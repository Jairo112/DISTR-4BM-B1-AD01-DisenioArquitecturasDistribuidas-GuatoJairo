using System.ComponentModel.DataAnnotations;

namespace Pedidos.Api.DTOs;

public class PedidoCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del cliente debe ser mayor que cero.")]
    public int IdCliente { get; set; }

    // La colección se valida en el controlador para dar el mensaje específico si falta o está vacía.
    public List<DetallePedidoCrearDto?>? Detalles { get; set; }
}
