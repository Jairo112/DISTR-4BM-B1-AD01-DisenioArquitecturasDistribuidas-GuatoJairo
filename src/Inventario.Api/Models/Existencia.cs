namespace Inventario.Api.Models;

public class Existencia
{
    public int IdExistencia { get; set; }
    // FK LÓGICA: IdProducto apunta a Producto en CatalogoDB_AD01 (otro microservicio, otra base). No hay FK en SQL; se valida llamando a Catalogo.Api por HTTP.
    public int IdProducto { get; set; }
    public int IdBodega { get; set; }
    public int Stock { get; set; }
    public Bodega Bodega { get; set; } = null!;
}
