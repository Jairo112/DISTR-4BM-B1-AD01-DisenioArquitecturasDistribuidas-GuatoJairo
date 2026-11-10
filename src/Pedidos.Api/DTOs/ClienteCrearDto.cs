using System.ComponentModel.DataAnnotations;
using Pedidos.Api.Validaciones;

namespace Pedidos.Api.DTOs;

public class ClienteCrearDto
{
    [Required(ErrorMessage = "Los nombres del cliente son obligatorios.")]
    [StringLength(150, ErrorMessage = "Los nombres no pueden superar los 150 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = true, ErrorMessage = "La cédula es obligatoria.")]
    [CedulaEcuatoriana]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido (ejemplo: nombre@dominio.com).")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;
}
