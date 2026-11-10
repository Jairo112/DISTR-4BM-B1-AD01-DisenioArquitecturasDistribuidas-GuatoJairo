namespace Pedidos.Api.DTOs;

public class ProductoCatalogoDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Estado { get; set; } = string.Empty;
}
