namespace Pedidos.Api.DTOs;

public class PedidoDto
{
    public int IdPedido { get; set; }
    public int IdCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<DetallePedidoDto> Detalles { get; set; } = new();
}
