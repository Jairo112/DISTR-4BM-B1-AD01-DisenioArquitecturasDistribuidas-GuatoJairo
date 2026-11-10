using System.ComponentModel.DataAnnotations;

namespace Pedidos.Api.DTOs;

public class PedidoEstadoDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(PENDIENTE|CONFIRMADO|ENTREGADO|ANULADO)$", ErrorMessage = "El estado debe ser PENDIENTE, CONFIRMADO, ENTREGADO o ANULADO.")]
    public string Estado { get; set; } = string.Empty;
}
