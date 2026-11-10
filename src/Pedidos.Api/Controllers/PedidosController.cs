using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Clients;
using Pedidos.Api.Data;
using Pedidos.Api.DTOs;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly PedidosDbContext _db;
    private readonly CatalogoClient _catalogo;

    public PedidosController(PedidosDbContext db, CatalogoClient catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<ActionResult<List<PedidoDto>>> ObtenerTodos(
        [FromQuery] int? idCliente, [FromQuery] string? estado)
    {
        var consulta = ConsultarPedidos().AsNoTracking();
        if (idCliente.HasValue)
        {
            consulta = consulta.Where(p => p.IdCliente == idCliente.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoNormalizado = estado.Trim().ToUpperInvariant();
            consulta = consulta.Where(p => p.Estado == estadoNormalizado);
        }

        var pedidos = await consulta.ToListAsync();
        return Ok(pedidos.Select(ConvertirADto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoDto>> ObtenerPorId(int id)
    {
        var pedido = await ConsultarPedidos().AsNoTracking().FirstOrDefaultAsync(p => p.IdPedido == id);
        if (pedido is null)
        {
            return NotFound(new { mensaje = "Pedido no encontrado." });
        }

        return Ok(ConvertirADto(pedido));
    }

    [HttpPost]
    public async Task<ActionResult<PedidoDto>> Crear(PedidoCrearDto datos)
    {
        if (datos.Detalles is null || datos.Detalles.Count == 0)
        {
            return BadRequest(new { mensaje = "El pedido debe tener al menos un detalle." });
        }

        var cliente = await _db.Clientes.FindAsync(datos.IdCliente);
        if (cliente is null)
        {
            return BadRequest(new { mensaje = "El cliente indicado no existe." });
        }

        var pedido = new Pedido
        {
            IdCliente = datos.IdCliente,
            Cliente = cliente,
            Fecha = DateTime.Now,
            Estado = "PENDIENTE"
        };

        try
        {
            foreach (var detalle in datos.Detalles)
            {
                if (detalle is null)
                {
                    return BadRequest(new { mensaje = "Los detalles del pedido no pueden ser nulos." });
                }

                if (detalle.Cantidad <= 0)
                {
                    return BadRequest(new { mensaje = "La cantidad debe ser mayor que cero." });
                }

                var producto = await _catalogo.ObtenerProductoAsync(detalle.IdProducto);
                if (producto is null)
                {
                    return BadRequest(new { mensaje = $"El producto {detalle.IdProducto} no existe en Catálogo." });
                }

                if (producto.Estado != "ACTIVO")
                {
                    return BadRequest(new { mensaje = $"El producto {detalle.IdProducto} está INACTIVO y no se puede pedir." });
                }

                // El precio queda guardado como referencia histórica del momento de la compra.
                pedido.Detalles.Add(new DetallePedido
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = producto.Precio
                });
            }
        }
        catch (CatalogoNoDisponibleException)
        {
            return StatusCode(503, new { mensaje = "El servicio de Catálogo no está disponible. Intente más tarde." });
        }

        pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        // EF guarda el pedido y todos sus detalles de forma atómica en una sola operación.
        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = pedido.IdPedido }, ConvertirADto(pedido));
    }

    [HttpPut("{id:int}/estado")]
    public async Task<ActionResult<PedidoDto>> CambiarEstado(int id, PedidoEstadoDto datos)
    {
        // La lectura y el cambio se protegen frente a otras transiciones o borrados concurrentes.
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var pedido = await ConsultarPedidos().FirstOrDefaultAsync(p => p.IdPedido == id);
        if (pedido is null)
        {
            return NotFound(new { mensaje = "Pedido no encontrado." });
        }

        var transicionPermitida = (pedido.Estado, datos.Estado) switch
        {
            ("PENDIENTE", "CONFIRMADO") => true,
            ("CONFIRMADO", "ENTREGADO") => true,
            ("PENDIENTE", "ANULADO") => true,
            ("CONFIRMADO", "ANULADO") => true,
            _ => false
        };

        if (!transicionPermitida)
        {
            return BadRequest(new { mensaje = $"No se puede cambiar el estado de {pedido.Estado} a {datos.Estado}." });
        }

        pedido.Estado = datos.Estado;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ConvertirADto(pedido));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var pedido = await _db.Pedidos.FindAsync(id);
        if (pedido is null)
        {
            return NotFound(new { mensaje = "Pedido no encontrado." });
        }

        if (pedido.Estado != "PENDIENTE")
        {
            return BadRequest(new { mensaje = "Solo se puede eliminar un pedido PENDIENTE." });
        }

        // La relación Cascade de SQL Server elimina también los detalles.
        _db.Pedidos.Remove(pedido);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    private IQueryable<Pedido> ConsultarPedidos()
    {
        return _db.Pedidos.Include(p => p.Cliente).Include(p => p.Detalles);
    }

    private static PedidoDto ConvertirADto(Pedido pedido)
    {
        return new PedidoDto
        {
            IdPedido = pedido.IdPedido,
            IdCliente = pedido.IdCliente,
            NombreCliente = pedido.Cliente.Nombres,
            Fecha = pedido.Fecha,
            Total = pedido.Total,
            Estado = pedido.Estado,
            Detalles = pedido.Detalles.OrderBy(d => d.IdDetalle).Select(d => new DetallePedidoDto
            {
                IdDetalle = d.IdDetalle,
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            }).ToList()
        };
    }
}
