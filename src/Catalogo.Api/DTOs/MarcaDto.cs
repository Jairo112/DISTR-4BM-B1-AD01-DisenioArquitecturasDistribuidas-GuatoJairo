namespace Catalogo.Api.DTOs;

public class MarcaDto
{
    public int IdMarca { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? PaisOrigen { get; set; }
    public string Estado { get; set; } = "ACTIVO";
}
