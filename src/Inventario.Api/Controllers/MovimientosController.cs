using Inventario.Api.Clients;
using Inventario.Api.Data;
using Inventario.Api.DTOs;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/movimientos")]
public class MovimientosController : ControllerBase
{
    private readonly InventarioDbContext _db;
    private readonly CatalogoClient _catalogo;

    public MovimientosController(InventarioDbContext db, CatalogoClient catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<ActionResult<List<MovimientoDto>>> ObtenerTodos(
        [FromQuery] int? idProducto, [FromQuery] int? idBodega, [FromQuery] string? tipo)
    {
        var consulta = ConsultarMovimientos();
        if (idProducto.HasValue)
        {
            consulta = consulta.Where(m => m.IdProducto == idProducto.Value);
        }

        if (idBodega.HasValue)
        {
            consulta = consulta.Where(m => m.IdBodega == idBodega.Value);
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipoNormalizado = tipo.Trim().ToUpperInvariant();
            consulta = consulta.Where(m => m.Tipo == tipoNormalizado);
        }

        return Ok(await consulta.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovimientoDto>> ObtenerPorId(int id)
    {
        var movimiento = await ConsultarMovimientos().FirstOrDefaultAsync(m => m.IdMovimiento == id);
        if (movimiento is null)
        {
            return NotFound(new { mensaje = "Movimiento no encontrado." });
        }

        return Ok(movimiento);
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoDto>> Crear(MovimientoCrearDto datos)
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

        // El movimiento y el stock se guardan juntos; sin commit, disponer la transacción revierte todo.
        // SERIALIZABLE protege también la lectura del stock y de las reservas ante otras escrituras.
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var existencia = await _db.Existencias
            .FirstOrDefaultAsync(e => e.IdProducto == datos.IdProducto && e.IdBodega == datos.IdBodega);

        if (datos.Tipo == "SALIDA")
        {
            var reservado = await _db.Reservas
                .Where(r => r.IdProducto == datos.IdProducto && r.IdBodega == datos.IdBodega && r.Estado == "ACTIVA")
                .SumAsync(r => (int?)r.Cantidad) ?? 0;
            var disponible = (existencia?.Stock ?? 0) - reservado;
            if (existencia is null || disponible < datos.Cantidad)
            {
                return BadRequest(new { mensaje = $"Stock insuficiente. Disponible: {disponible}, solicitado: {datos.Cantidad}." });
            }

            existencia.Stock -= datos.Cantidad;
        }
        else
        {
            if (existencia is null)
            {
                existencia = new Existencia
                {
                    IdProducto = datos.IdProducto,
                    IdBodega = datos.IdBodega,
                    Stock = 0
                };
                _db.Existencias.Add(existencia);
            }

            if (datos.Cantidad > int.MaxValue - existencia.Stock)
            {
                return BadRequest(new { mensaje = "La entrada supera el stock máximo permitido." });
            }

            existencia.Stock += datos.Cantidad;
        }

        var movimiento = new MovimientoInventario
        {
            IdProducto = datos.IdProducto,
            IdBodega = datos.IdBodega,
            Tipo = datos.Tipo,
            Cantidad = datos.Cantidad,
            Fecha = DateTime.Now
        };
        _db.Movimientos.Add(movimiento);
        await _db.SaveChangesAsync();

        var resultado = await ConsultarMovimientos().FirstAsync(m => m.IdMovimiento == movimiento.IdMovimiento);
        await tx.CommitAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = movimiento.IdMovimiento }, resultado);
    }

    private IQueryable<MovimientoDto> ConsultarMovimientos()
    {
        return _db.Movimientos.AsNoTracking().Select(m => new MovimientoDto
        {
            IdMovimiento = m.IdMovimiento,
            IdProducto = m.IdProducto,
            IdBodega = m.IdBodega,
            Tipo = m.Tipo,
            Cantidad = m.Cantidad,
            Fecha = m.Fecha
        });
    }
}
