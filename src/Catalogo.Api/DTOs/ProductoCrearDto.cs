using System.ComponentModel.DataAnnotations;

namespace Catalogo.Api.DTOs;

public class ProductoCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El identificador de la categoría debe ser mayor que cero.")]
    public int IdCategoria { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El identificador de la marca debe ser mayor que cero.")]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre del producto no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [StringLength(30, ErrorMessage = "El SKU no puede superar los 30 caracteres.")]
    public string SKU { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "El precio debe ser mayor que 0.")]
    public decimal Precio { get; set; }

    [RegularExpression("^(ACTIVO|INACTIVO)$", ErrorMessage = "El estado debe ser ACTIVO o INACTIVO.")]
    public string? Estado { get; set; }
}
