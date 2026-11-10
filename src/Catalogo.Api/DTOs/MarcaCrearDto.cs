using System.ComponentModel.DataAnnotations;

namespace Catalogo.Api.DTOs;

public class MarcaCrearDto
{
    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre de la marca no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "El país de origen no puede superar los 60 caracteres.")]
    public string? PaisOrigen { get; set; }
}
