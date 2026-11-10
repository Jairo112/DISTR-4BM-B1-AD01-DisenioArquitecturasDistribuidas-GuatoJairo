namespace Inventario.Api.DTOs;

public class BodegaDto
{
    public int IdBodega { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string? Direccion { get; set; }
}
