namespace Pedidos.Api.DTOs;

public class ClienteDto
{
    public int IdCliente { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
}
