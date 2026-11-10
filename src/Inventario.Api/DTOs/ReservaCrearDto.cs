using System.ComponentModel.DataAnnotations;

namespace Inventario.Api.DTOs;

public class ReservaCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del producto debe ser mayor que cero.")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El identificador de la bodega debe ser mayor que cero.")]
    public int IdBodega { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { get; set; }
}
