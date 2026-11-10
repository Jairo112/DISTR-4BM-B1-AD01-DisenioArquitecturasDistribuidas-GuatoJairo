namespace Pedidos.Api.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
