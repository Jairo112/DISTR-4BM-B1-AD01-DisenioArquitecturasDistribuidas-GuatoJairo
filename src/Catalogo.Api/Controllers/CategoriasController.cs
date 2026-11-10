using Catalogo.Api.Data;
using Catalogo.Api.DTOs;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly CatalogoDbContext _contexto;

    public CategoriasController(CatalogoDbContext contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoriaDto>>> ObtenerTodas()
    {
        var categorias = await _contexto.Categorias.AsNoTracking()
            .Select(c => new CategoriaDto
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Estado = c.Estado
            })
            .ToListAsync();

        return Ok(categorias);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> ObtenerPorId(int id)
    {
        var categoria = await _contexto.Categorias.AsNoTracking()
            .Where(c => c.IdCategoria == id)
            .Select(c => new CategoriaDto
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Estado = c.Estado
            })
            .FirstOrDefaultAsync();

        if (categoria is null)
        {
            return NotFound(new { mensaje = "Categoría no encontrada." });
        }

        return Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Crear(CategoriaCrearDto datos)
    {
        var categoria = new Categoria
        {
            Nombre = datos.Nombre,
            Descripcion = datos.Descripcion
        };

        _contexto.Categorias.Add(categoria);
        await _contexto.SaveChangesAsync();

        var resultado = new CategoriaDto
        {
            IdCategoria = categoria.IdCategoria,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Estado = categoria.Estado
        };

        return CreatedAtAction(nameof(ObtenerPorId), new { id = categoria.IdCategoria }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> Actualizar(int id, CategoriaCrearDto datos)
    {
        var categoria = await _contexto.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound(new { mensaje = "Categoría no encontrada." });
        }

        categoria.Nombre = datos.Nombre;
        categoria.Descripcion = datos.Descripcion;
        await _contexto.SaveChangesAsync();

        return Ok(new CategoriaDto
        {
            IdCategoria = categoria.IdCategoria,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Estado = categoria.Estado
        });
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> ActualizarDescripcion(
    int id, CategoriaPatchDto datos)
    {
        var categoria = await _contexto.Categorias.FindAsync(id);

        if (categoria is null)
        {
            return NotFound(new
            {
                mensaje = "Categoría no encontrada."
            });
        }

        categoria.Descripcion = datos.Descripcion;
        await _contexto.SaveChangesAsync();

        return Ok(new CategoriaDto
        {
            IdCategoria = categoria.IdCategoria,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Estado = categoria.Estado
        });
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<ActionResult<CategoriaDto>> ActualizarEstado(
        int id, CategoriaEstadoDto datos)
    {
        var categoria = await _contexto.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound(new { mensaje = "Categoría no encontrada." });
        }

        categoria.Estado = datos.Estado;
        await _contexto.SaveChangesAsync();

        return Ok(new CategoriaDto
        {
            IdCategoria = categoria.IdCategoria,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Estado = categoria.Estado
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var categoria = await _contexto.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound(new { mensaje = "Categoría no encontrada." });
        }

        if (await _contexto.Productos.AnyAsync(p => p.IdCategoria == id))
        {
            return BadRequest(new { mensaje = "No se puede eliminar la categoría porque tiene productos asociados." });
        }

        _contexto.Categorias.Remove(categoria);
        await _contexto.SaveChangesAsync();

        return Ok(new { mensaje = "Categoría eliminada correctamente." });
    }
}
