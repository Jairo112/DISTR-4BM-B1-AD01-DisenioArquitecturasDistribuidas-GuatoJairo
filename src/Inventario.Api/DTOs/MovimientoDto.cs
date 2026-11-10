namespace Inventario.Api.DTOs;

public class MovimientoDto
{
    public int IdMovimiento { get; set; }
    public int IdProducto { get; set; }
    public int IdBodega { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }
}
