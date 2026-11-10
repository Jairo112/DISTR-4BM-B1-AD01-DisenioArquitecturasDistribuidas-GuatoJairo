namespace Catalogo.Api.DTOs;

public class ProductoDto
{
    public int IdProducto { get; set; }
    public int IdCategoria { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public int IdMarca { get; set; }
    public string NombreMarca { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Estado { get; set; } = string.Empty;
}
