namespace Inventario.Api.DTOs;

public class ExistenciaDto
{
    public int IdExistencia { get; set; }
    public int IdProducto { get; set; }
    public int IdBodega { get; set; }
    public int Stock { get; set; }
    public int StockReservado { get; set; }
    public int StockDisponible { get; set; }
}
