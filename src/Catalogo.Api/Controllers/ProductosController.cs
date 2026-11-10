using Microsoft.Data.SqlClient;
using Catalogo.Api.Data;
using Catalogo.Api.DTOs;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly CatalogoDbContext _contexto;

    public ProductosController(CatalogoDbContext contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductoDto>>> ObtenerTodos(
        [FromQuery] int? idCategoria,
        [FromQuery] int? idMarca,
        [FromQuery] string? estado)
    {
        var consulta = ConsultarProductos();

        if (idCategoria.HasValue)
        {
            consulta = consulta.Where(p => p.IdCategoria == idCategoria.Value);
        }

        if (idMarca.HasValue)
        {
            consulta = consulta.Where(p => p.IdMarca == idMarca.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoNormalizado = estado.Trim().ToUpper();
            consulta = consulta.Where(p => p.Estado == estadoNormalizado);
        }

        return Ok(await consulta.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(int id)
    {
        var producto = await ConsultarProductos().FirstOrDefaultAsync(p => p.IdProducto == id);
        if (producto is null)
        {
            return NotFound(new { mensaje = "Producto no encontrado." });
        }

        return Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Crear(ProductoCrearDto datos)
    {
        if (!await _contexto.Categorias.AnyAsync(c => c.IdCategoria == datos.IdCategoria))
        {
            return BadRequest(new { mensaje = "La categoría indicada no existe." });
        }

        if (!await _contexto.Marcas.AnyAsync(m => m.IdMarca == datos.IdMarca))
        {
            return BadRequest(new { mensaje = "La marca indicada no existe." });
        }

        var skuNormalizado = datos.SKU.Trim().ToUpper();
        if (await _contexto.Productos.AnyAsync(p => p.SKU == skuNormalizado))
        {
            return BadRequest(new { mensaje = "Ya existe un producto con el SKU indicado." });
        }

        var producto = new Producto
        {
            IdCategoria = datos.IdCategoria,
            IdMarca = datos.IdMarca,
            Nombre = datos.Nombre,
            SKU = skuNormalizado,
            Precio = datos.Precio,
            Estado = datos.Estado?.Trim().ToUpper() ?? "ACTIVO"
        };

        _contexto.Productos.Add(producto);
        await _contexto.SaveChangesAsync();

        var resultado = await ConsultarProductos().FirstAsync(p => p.IdProducto == producto.IdProducto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.IdProducto }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductoDto>> Actualizar(int id, ProductoCrearDto datos)
    {
        var producto = await _contexto.Productos.FindAsync(id);
        if (producto is null)
        {
            return NotFound(new { mensaje = "Producto no encontrado." });
        }

        if (!await _contexto.Categorias.AnyAsync(c => c.IdCategoria == datos.IdCategoria))
        {
            return BadRequest(new { mensaje = "La categoría indicada no existe." });
        }

        if (!await _contexto.Marcas.AnyAsync(m => m.IdMarca == datos.IdMarca))
        {
            return BadRequest(new { mensaje = "La marca indicada no existe." });
        }

        var skuNormalizado = datos.SKU.Trim().ToUpper();
        if (await _contexto.Productos.AnyAsync(p => p.SKU == skuNormalizado && p.IdProducto != id))
        {
            return BadRequest(new { mensaje = "Ya existe un producto con el SKU indicado." });
        }

        producto.IdCategoria = datos.IdCategoria;
        producto.IdMarca = datos.IdMarca;
        producto.Nombre = datos.Nombre;
        producto.SKU = skuNormalizado;
        producto.Precio = datos.Precio;
        producto.Estado = datos.Estado?.Trim().ToUpper() ?? "ACTIVO";
        await _contexto.SaveChangesAsync();

        var resultado = await ConsultarProductos().FirstAsync(p => p.IdProducto == id);
        return Ok(resultado);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<ProductoDto>> ActualizarParcial(int id, ProductoPatchDto datos)
    {
        var producto = await _contexto.Productos.FindAsync(id);
        if (producto is null)
        {
            return NotFound(new { mensaje = "Producto no encontrado." });
        }

        if (datos.SkuEnviado)
        {
            var skuNormalizado = datos.SKU!.Trim().ToUpper();
            if (await _contexto.Productos.AnyAsync(p => p.SKU == skuNormalizado && p.IdProducto != id))
            {
                return BadRequest(new { mensaje = "Ya existe un producto con el SKU indicado." });
            }
            producto.SKU = skuNormalizado;
        }
        if (datos.NombreEnviado)
        {
            producto.Nombre = datos.Nombre!;
        }
        if (datos.PrecioEnviado)
        {
            producto.Precio = datos.Precio!.Value;
        }
        if (datos.EstadoEnviado)
        {
            producto.Estado = datos.Estado!;
        }

        try
        {
            await _contexto.SaveChangesAsync();
        }
        catch (DbUpdateException excepcion) when (
            excepcion.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return BadRequest(new { mensaje = "Ya existe un producto con el SKU indicado." });
        }

        var resultado = await ConsultarProductos().FirstAsync(p => p.IdProducto == id);
        return Ok(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _contexto.Productos.FindAsync(id);
        if (producto is null)
        {
            return NotFound(new { mensaje = "Producto no encontrado." });
        }

        // Inventario y Pedidos referencian IdProducto por FK lógica;
        // la baja lógica conserva esas referencias sin borrar físicamente el producto.
        producto.Estado = "INACTIVO";
        await _contexto.SaveChangesAsync();

        return Ok(new { mensaje = "Producto dado de baja (INACTIVO)" });
    }

    private IQueryable<ProductoDto> ConsultarProductos()
    {
        return _contexto.Productos.AsNoTracking()
            .Select(p => new ProductoDto
            {
                IdProducto = p.IdProducto,
                IdCategoria = p.IdCategoria,
                NombreCategoria = p.Categoria.Nombre,
                IdMarca = p.IdMarca,
                NombreMarca = p.Marca.Nombre,
                Nombre = p.Nombre,
                SKU = p.SKU,
                Precio = p.Precio,
                Estado = p.Estado
            });
    }
}
