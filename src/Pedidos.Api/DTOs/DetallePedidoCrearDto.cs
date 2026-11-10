using System.ComponentModel.DataAnnotations;

namespace Pedidos.Api.DTOs;

public class DetallePedidoCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del producto debe ser mayor que cero.")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { get; set; }
}
