using System.ComponentModel.DataAnnotations;

namespace Inventario.Api.DTOs;

public class ExistenciaCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del producto debe ser mayor que cero.")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El identificador de la bodega debe ser mayor que cero.")]
    public int IdBodega { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock inicial debe ser mayor o igual que cero.")]
    public int Stock { get; set; }
}
