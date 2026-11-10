using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Catalogo.Api.DTOs
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class CategoriaPatchDto
    {
        [JsonRequired]
        [StringLength(250,ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
        
        public string? Descripcion { get; set; }
    }
}
