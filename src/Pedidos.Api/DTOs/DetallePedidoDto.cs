namespace Pedidos.Api.DTOs;

public class DetallePedidoDto
{
    public int IdDetalle { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
