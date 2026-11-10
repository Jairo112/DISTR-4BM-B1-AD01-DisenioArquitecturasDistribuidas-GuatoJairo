using Catalogo.Api.Data;
using Catalogo.Api.DTOs;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/marcas")]
public class MarcasController : ControllerBase
{
    private readonly CatalogoDbContext _contexto;

    public MarcasController(CatalogoDbContext contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<List<MarcaDto>>> ObtenerTodas()
    {
        var marcas = await _contexto.Marcas.AsNoTracking()
            .Select(m => new MarcaDto
            {
                IdMarca = m.IdMarca,
                Nombre = m.Nombre,
                PaisOrigen = m.PaisOrigen,
                Estado = m.Estado
            })
            .ToListAsync();

        return Ok(marcas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MarcaDto>> ObtenerPorId(int id)
    {
        var marca = await _contexto.Marcas.AsNoTracking()
            .Where(m => m.IdMarca == id)
            .Select(m => new MarcaDto
            {
                IdMarca = m.IdMarca,
                Nombre = m.Nombre,
                PaisOrigen = m.PaisOrigen,
                Estado = m.Estado
            })
            .FirstOrDefaultAsync();

        if (marca is null)
        {
            return NotFound(new { mensaje = "Marca no encontrada." });
        }

        return Ok(marca);
    }

    [HttpPost]
    public async Task<ActionResult<MarcaDto>> Crear(MarcaCrearDto datos)
    {
        var marca = new Marca
        {
            Nombre = datos.Nombre,
            PaisOrigen = datos.PaisOrigen
        };

        _contexto.Marcas.Add(marca);
        await _contexto.SaveChangesAsync();

        var resultado = new MarcaDto
        {
            IdMarca = marca.IdMarca,
            Nombre = marca.Nombre,
            PaisOrigen = marca.PaisOrigen,
            Estado = marca.Estado
        };

        return CreatedAtAction(nameof(ObtenerPorId), new { id = marca.IdMarca }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MarcaDto>> Actualizar(int id, MarcaCrearDto datos)
    {
        var marca = await _contexto.Marcas.FindAsync(id);
        if (marca is null)
        {
            return NotFound(new { mensaje = "Marca no encontrada." });
        }

        marca.Nombre = datos.Nombre;
        marca.PaisOrigen = datos.PaisOrigen;
        await _contexto.SaveChangesAsync();

        return Ok(new MarcaDto
        {
            IdMarca = marca.IdMarca,
            Nombre = marca.Nombre,
            PaisOrigen = marca.PaisOrigen,
            Estado = marca.Estado
        });
    }

    [HttpPatch("{id:int}/pais-origen")]
    public async Task<ActionResult<MarcaDto>> ActualizarPaisOrigen(int id, MarcaPaisDto datos)
    {
        var marca = await _contexto.Marcas.FindAsync(id);
        if (marca is null)
        {
            return NotFound(new { mensaje = "Marca no encontrada." });
        }

        marca.PaisOrigen = datos.PaisOrigen;
        await _contexto.SaveChangesAsync();

        return Ok(new MarcaDto
        {
            IdMarca = marca.IdMarca,
            Nombre = marca.Nombre,
            PaisOrigen = marca.PaisOrigen,
            Estado = marca.Estado
        });
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<ActionResult<MarcaDto>> ActualizarEstado(int id, MarcaEstadoDto datos)
    {
        var marca = await _contexto.Marcas.FindAsync(id);
        if (marca is null)
        {
            return NotFound(new { mensaje = "Marca no encontrada." });
        }

        marca.Estado = datos.Estado;
        await _contexto.SaveChangesAsync();

        return Ok(new MarcaDto
        {
            IdMarca = marca.IdMarca,
            Nombre = marca.Nombre,
            PaisOrigen = marca.PaisOrigen,
            Estado = marca.Estado
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var marca = await _contexto.Marcas.FindAsync(id);
        if (marca is null)
        {
            return NotFound(new { mensaje = "Marca no encontrada." });
        }

        if (await _contexto.Productos.AnyAsync(p => p.IdMarca == id))
        {
            return BadRequest(new { mensaje = "No se puede eliminar la marca porque tiene productos asociados." });
        }

        _contexto.Marcas.Remove(marca);
        await _contexto.SaveChangesAsync();

        return Ok(new { mensaje = "Marca eliminada correctamente." });
    }
}
