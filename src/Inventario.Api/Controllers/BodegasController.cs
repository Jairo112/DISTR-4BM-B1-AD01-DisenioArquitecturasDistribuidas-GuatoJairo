using Inventario.Api.Data;
using Inventario.Api.DTOs;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/bodegas")]
public class BodegasController : ControllerBase
{
    private readonly InventarioDbContext _db;

    public BodegasController(InventarioDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<BodegaDto>>> ObtenerTodas()
    {
        return Ok(await ConsultarBodegas().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BodegaDto>> ObtenerPorId(int id)
    {
        var bodega = await ConsultarBodegas().FirstOrDefaultAsync(b => b.IdBodega == id);
        if (bodega is null)
        {
            return NotFound(new { mensaje = "Bodega no encontrada." });
        }

        return Ok(bodega);
    }

    [HttpPost]
    public async Task<ActionResult<BodegaDto>> Crear(BodegaCrearDto datos)
    {
        var bodega = new Bodega
        {
            Nombre = datos.Nombre,
            Ciudad = datos.Ciudad,
            Direccion = datos.Direccion
        };
        _db.Bodegas.Add(bodega);
        await _db.SaveChangesAsync();

        var resultado = await ConsultarBodegas().FirstAsync(b => b.IdBodega == bodega.IdBodega);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = bodega.IdBodega }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BodegaDto>> Actualizar(int id, BodegaCrearDto datos)
    {
        var bodega = await _db.Bodegas.FindAsync(id);
        if (bodega is null)
        {
            return NotFound(new { mensaje = "Bodega no encontrada." });
        }

        bodega.Nombre = datos.Nombre;
        bodega.Ciudad = datos.Ciudad;
        bodega.Direccion = datos.Direccion;
        await _db.SaveChangesAsync();

        return Ok(await ConsultarBodegas().FirstAsync(b => b.IdBodega == id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var bodega = await _db.Bodegas.FindAsync(id);
        if (bodega is null)
        {
            return NotFound(new { mensaje = "Bodega no encontrada." });
        }

        if (await _db.Existencias.AnyAsync(e => e.IdBodega == id)
            || await _db.Movimientos.AnyAsync(m => m.IdBodega == id)
            || await _db.Reservas.AnyAsync(r => r.IdBodega == id))
        {
            return BadRequest(new
            {
                mensaje = "No se puede eliminar la bodega porque tiene existencias, movimientos o reservas registradas."
            });
        }

        _db.Bodegas.Remove(bodega);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    private IQueryable<BodegaDto> ConsultarBodegas()
    {
        return _db.Bodegas.AsNoTracking().Select(b => new BodegaDto
        {
            IdBodega = b.IdBodega,
            Nombre = b.Nombre,
            Ciudad = b.Ciudad,
            Direccion = b.Direccion
        });
    }
}
