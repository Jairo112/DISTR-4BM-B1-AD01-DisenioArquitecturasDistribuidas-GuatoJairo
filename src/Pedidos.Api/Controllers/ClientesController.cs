using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.DTOs;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly PedidosDbContext _db;

    public ClientesController(PedidosDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClienteDto>>> ObtenerTodos()
    {
        return Ok(await ConsultarClientes().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> ObtenerPorId(int id)
    {
        var cliente = await ConsultarClientes().FirstOrDefaultAsync(c => c.IdCliente == id);
        if (cliente is null)
        {
            return NotFound(new { mensaje = "Cliente no encontrado." });
        }

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Crear(ClienteCrearDto datos)
    {
        var mensaje = await ValidarDuplicadosAsync(datos);
        if (mensaje is not null)
        {
            return BadRequest(new { mensaje });
        }

        var cliente = new Cliente
        {
            Nombres = datos.Nombres,
            Cedula = datos.Cedula,
            Correo = datos.Correo
        };
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        var resultado = await ConsultarClientes().FirstAsync(c => c.IdCliente == cliente.IdCliente);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.IdCliente }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Actualizar(int id, ClienteCrearDto datos)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound(new { mensaje = "Cliente no encontrado." });
        }

        var mensaje = await ValidarDuplicadosAsync(datos, id);
        if (mensaje is not null)
        {
            return BadRequest(new { mensaje });
        }

        cliente.Nombres = datos.Nombres;
        cliente.Cedula = datos.Cedula;
        cliente.Correo = datos.Correo;
        await _db.SaveChangesAsync();

        return Ok(await ConsultarClientes().FirstAsync(c => c.IdCliente == id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        // Evita que se registre un pedido entre la comprobación y el borrado del cliente.
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound(new { mensaje = "Cliente no encontrado." });
        }

        if (await _db.Pedidos.AnyAsync(p => p.IdCliente == id))
        {
            return BadRequest(new { mensaje = "No se puede eliminar el cliente porque tiene pedidos registrados." });
        }

        _db.Clientes.Remove(cliente);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    private async Task<string?> ValidarDuplicadosAsync(ClienteCrearDto datos, int? idExcluir = null)
    {
        var consulta = _db.Clientes.AsNoTracking();
        if (idExcluir.HasValue)
        {
            consulta = consulta.Where(c => c.IdCliente != idExcluir.Value);
        }

        if (await consulta.AnyAsync(c => c.Cedula == datos.Cedula))
        {
            return "Ya existe un cliente con esa cédula.";
        }

        if (await consulta.AnyAsync(c => c.Correo == datos.Correo))
        {
            return "Ya existe un cliente con ese correo.";
        }

        return null;
    }

    private IQueryable<ClienteDto> ConsultarClientes()
    {
        return _db.Clientes.AsNoTracking().Select(c => new ClienteDto
        {
            IdCliente = c.IdCliente,
            Nombres = c.Nombres,
            Cedula = c.Cedula,
            Correo = c.Correo
        });
    }
}
