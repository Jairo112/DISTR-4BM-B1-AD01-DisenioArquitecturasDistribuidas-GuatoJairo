namespace Catalogo.Api.Models;

public class Marca
{
    public int IdMarca { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? PaisOrigen { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
