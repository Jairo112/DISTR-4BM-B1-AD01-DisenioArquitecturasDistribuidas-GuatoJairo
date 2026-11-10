using System.ComponentModel.DataAnnotations;

namespace Catalogo.Api.DTOs;

public class CategoriaEstadoDto
{
    [Required(ErrorMessage = "El estado de la categoría es obligatorio.")]
    [RegularExpression("^(ACTIVO|INACTIVO)$",
        ErrorMessage = "El estado debe ser ACTIVO o INACTIVO.")]
    public string Estado { get; set; } = string.Empty;
}
