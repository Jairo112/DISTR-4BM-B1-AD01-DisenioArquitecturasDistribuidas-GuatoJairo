using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Catalogo.Api.DTOs;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class CategoriaCrearDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre de la categoría no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    public string? Descripcion { get; set; }
}
