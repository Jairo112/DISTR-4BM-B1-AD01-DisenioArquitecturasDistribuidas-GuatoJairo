namespace Pedidos.Api.Models;

public class DetallePedido
{
    public int IdDetalle { get; set; }
    public int IdPedido { get; set; }
    // FK LÓGICA: IdProducto apunta a Producto en CatalogoDB_AD01 (otro microservicio, otra base). No hay FK en SQL; se valida llamando a Catalogo.Api por HTTP.
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public Pedido Pedido { get; set; } = null!;
}
