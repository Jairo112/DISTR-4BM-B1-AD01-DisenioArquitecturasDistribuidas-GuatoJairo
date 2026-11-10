using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Catalogo.Api.DTOs;

public class MarcaPaisDto
{
    [JsonRequired]
    [StringLength(60, ErrorMessage = "El país de origen no puede superar los 60 caracteres.")]
    public string? PaisOrigen { get; set; }
}
