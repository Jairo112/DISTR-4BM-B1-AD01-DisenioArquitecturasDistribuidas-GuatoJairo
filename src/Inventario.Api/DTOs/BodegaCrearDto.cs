using System.ComponentModel.DataAnnotations;

namespace Inventario.Api.DTOs;

public class BodegaCrearDto
{
    [Required(ErrorMessage = "El nombre de la bodega es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(80, ErrorMessage = "La ciudad no puede superar los 80 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
    public string? Direccion { get; set; }
}
