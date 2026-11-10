using Inventario.Api.Clients;
using Inventario.Api.Data;
using Inventario.Api.DTOs;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/existencias")]
public class ExistenciasController : ControllerBase
{
    private readonly InventarioDbContext _db;
    private readonly CatalogoClient _catalogo;

    public ExistenciasController(InventarioDbContext db, CatalogoClient catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExistenciaDto>>> ObtenerTodas(
        [FromQuery] int? idProducto, [FromQuery] int? idBodega)
    {
        var consulta = ConsultarExistencias();
        if (idProducto.HasValue)
        {
            consulta = consulta.Where(e => e.IdProducto == idProducto.Value);
        }

        if (idBodega.HasValue)
        {
            consulta = consulta.Where(e => e.IdBodega == idBodega.Value);
        }

        return Ok(await consulta.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExistenciaDto>> ObtenerPorId(int id)
    {
        var existencia = await ConsultarExistencias().FirstOrDefaultAsync(e => e.IdExistencia == id);
        if (existencia is null)
        {
            return NotFound(new { mensaje = "Existencia no encontrada." });
        }

        return Ok(existencia);
    }

    [HttpPost]
    public async Task<ActionResult<ExistenciaDto>> Crear(ExistenciaCrearDto datos)
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

        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if (await _db.Existencias.AnyAsync(e => e.IdProducto == datos.IdProducto && e.IdBodega == datos.IdBodega))
        {
            return BadRequest(new { mensaje = "Ya existe una existencia para el producto en la bodega indicada." });
        }

        var existencia = new Existencia
        {
            IdProducto = datos.IdProducto,
            IdBodega = datos.IdBodega,
            Stock = datos.Stock
        };
        _db.Existencias.Add(existencia);
        await _db.SaveChangesAsync();

        var resultado = await ConsultarExistencias().FirstAsync(e => e.IdExistencia == existencia.IdExistencia);
        await tx.CommitAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = existencia.IdExistencia }, resultado);
    }

    private IQueryable<ExistenciaDto> ConsultarExistencias()
    {
        // Solo las reservas ACTIVAS comprometen unidades que todavía están en stock.
        return _db.Existencias.AsNoTracking()
            .Select(e => new
            {
                e.IdExistencia,
                e.IdProducto,
                e.IdBodega,
                e.Stock,
                StockReservado = _db.Reservas
                    .Where(r => r.IdProducto == e.IdProducto && r.IdBodega == e.IdBodega && r.Estado == "ACTIVA")
                    .Sum(r => (int?)r.Cantidad) ?? 0
            })
            .Select(e => new ExistenciaDto
            {
                IdExistencia = e.IdExistencia,
                IdProducto = e.IdProducto,
                IdBodega = e.IdBodega,
                Stock = e.Stock,
                StockReservado = e.StockReservado,
                StockDisponible = e.Stock - e.StockReservado
            });
    }
}
