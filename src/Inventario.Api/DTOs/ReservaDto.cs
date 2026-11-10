namespace Inventario.Api.DTOs;

public class ReservaDto
{
    public int IdReserva { get; set; }
    public int IdProducto { get; set; }
    public int IdBodega { get; set; }
    public int Cantidad { get; set; }
    public string Estado { get; set; } = string.Empty;
}
