using Inventario.Api.Clients;
using Inventario.Api.Data;
using Inventario.Api.DTOs;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/reservas")]
public class ReservasController : ControllerBase
{
    private readonly InventarioDbContext _db;
    private readonly CatalogoClient _catalogo;

    public ReservasController(InventarioDbContext db, CatalogoClient catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReservaDto>>> ObtenerTodas(
        [FromQuery] int? idProducto, [FromQuery] int? idBodega, [FromQuery] string? estado)
    {
        var consulta = ConsultarReservas();
        if (idProducto.HasValue)
        {
            consulta = consulta.Where(r => r.IdProducto == idProducto.Value);
        }

        if (idBodega.HasValue)
        {
            consulta = consulta.Where(r => r.IdBodega == idBodega.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoNormalizado = estado.Trim().ToUpperInvariant();
            consulta = consulta.Where(r => r.Estado == estadoNormalizado);
        }

        return Ok(await consulta.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservaDto>> ObtenerPorId(int id)
    {
        var reserva = await ConsultarReservas().FirstOrDefaultAsync(r => r.IdReserva == id);
        if (reserva is null)
        {
            return NotFound(new { mensaje = "Reserva no encontrada." });
        }

        return Ok(reserva);
    }

    [HttpPost]
    public async Task<ActionResult<ReservaDto>> Crear(ReservaCrearDto datos)
    {
        if (!await _db.Bodegas.AnyAsync(b => b.IdBodega == datos.IdBodega))
        {
            return BadRequest(new { mensaje = "La bodega indicada no existe." });
        }

        try
        {
            if (!await _catalogo.ExisteProductoAsync(datos.IdProducto))
            {
                return BadRequest(new { mensaje = "El producto indicado no existe en Catálogo." });
            }
        }
        catch (CatalogoNoDisponibleException)
        {
            return StatusCode(503, new { mensaje = "El servicio de Catálogo no está disponible. Intente más tarde." });
        }

        // La comprobación y la reserva son atómicas para no comprometer dos veces las mismas unidades.
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var existencia = await _db.Existencias.AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdProducto == datos.IdProducto && e.IdBodega == datos.IdBodega);
        var reservado = await _db.Reservas
            .Where(r => r.IdProducto == datos.IdProducto && r.IdBodega == datos.IdBodega && r.Estado == "ACTIVA")
            .SumAsync(r => (int?)r.Cantidad) ?? 0;
        var disponible = (existencia?.Stock ?? 0) - reservado;
        if (disponible < datos.Cantidad)
        {
            return BadRequest(new { mensaje = $"Stock insuficiente. Disponible: {disponible}, solicitado: {datos.Cantidad}." });
        }

        var reserva = new ReservaStock
        {
            IdProducto = datos.IdProducto,
            IdBodega = datos.IdBodega,
            Cantidad = datos.Cantidad,
            Estado = "ACTIVA"
        };
        _db.Reservas.Add(reserva);
        await _db.SaveChangesAsync();

        var resultado = await ConsultarReservas().FirstAsync(r => r.IdReserva == reserva.IdReserva);
        await tx.CommitAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = reserva.IdReserva }, resultado);
    }

    [HttpPut("{id:int}/confirmar")]
    public async Task<ActionResult<ReservaDto>> Confirmar(int id)
    {
        // El cambio de estado, el descuento y la salida deben confirmarse o revertirse juntos.
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var reserva = await _db.Reservas.FirstOrDefaultAsync(r => r.IdReserva == id);
        if (reserva is null)
        {
            return NotFound(new { mensaje = "Reserva no encontrada." });
        }

        if (reserva.Estado != "ACTIVA")
        {
            return BadRequest(new { mensaje = "Solo se puede confirmar una reserva ACTIVA." });
        }

        var existencia = await _db.Existencias
            .FirstOrDefaultAsync(e => e.IdProducto == reserva.IdProducto && e.IdBodega == reserva.IdBodega);
        if (existencia is null || existencia.Stock < reserva.Cantidad)
        {
            return BadRequest(new { mensaje = $"Stock insuficiente. Disponible: {existencia?.Stock ?? 0}, solicitado: {reserva.Cantidad}." });
        }

        // Estas unidades ya estaban reservadas: se descuentan del stock físico, sin volver a reservarlas.
        reserva.Estado = "CONFIRMADA";
        existencia.Stock -= reserva.Cantidad;
        _db.Movimientos.Add(new MovimientoInventario
        {
            IdProducto = reserva.IdProducto,
            IdBodega = reserva.IdBodega,
            Tipo = "SALIDA",
            Cantidad = reserva.Cantidad,
            Fecha = DateTime.Now
        });
        await _db.SaveChangesAsync();

        var resultado = await ConsultarReservas().FirstAsync(r => r.IdReserva == id);
        await tx.CommitAsync();
        return Ok(resultado);
    }

    [HttpPut("{id:int}/cancelar")]
    public async Task<ActionResult<ReservaDto>> Cancelar(int id)
    {
        // La transacción evita que una confirmación y una cancelación cambien la misma reserva a la vez.
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var reserva = await _db.Reservas.FirstOrDefaultAsync(r => r.IdReserva == id);
        if (reserva is null)
        {
            return NotFound(new { mensaje = "Reserva no encontrada." });
        }

        if (reserva.Estado != "ACTIVA")
        {
            return BadRequest(new { mensaje = "Solo se puede cancelar una reserva ACTIVA." });
        }

        reserva.Estado = "CANCELADA";
        await _db.SaveChangesAsync();

        var resultado = await ConsultarReservas().FirstAsync(r => r.IdReserva == id);
        await tx.CommitAsync();
        return Ok(resultado);
    }

    private IQueryable<ReservaDto> ConsultarReservas()
    {
        return _db.Reservas.AsNoTracking().Select(r => new ReservaDto
        {
            IdReserva = r.IdReserva,
            IdProducto = r.IdProducto,
            IdBodega = r.IdBodega,
            Cantidad = r.Cantidad,
            Estado = r.Estado
        });
    }
}
