namespace Catalogo.Api.DTOs;

public class CategoriaDto
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = string.Empty;
}
