using System.ComponentModel.DataAnnotations;

namespace Inventario.Api.DTOs;

public class MovimientoCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del producto debe ser mayor que cero.")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El identificador de la bodega debe ser mayor que cero.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El tipo de movimiento es obligatorio.")]
    [StringLength(10, ErrorMessage = "El tipo no puede superar los 10 caracteres.")]
    [RegularExpression("^(ENTRADA|SALIDA)$", ErrorMessage = "El tipo debe ser ENTRADA o SALIDA.")]
    public string Tipo { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { get; set; }
}
