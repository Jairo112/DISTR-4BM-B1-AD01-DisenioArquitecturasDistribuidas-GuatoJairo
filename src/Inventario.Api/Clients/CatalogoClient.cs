using System.Net;

namespace Inventario.Api.Clients;

public class CatalogoClient
{
    private readonly HttpClient _cliente;

    public CatalogoClient(HttpClient cliente)
    {
        _cliente = cliente;
    }

    public async Task<bool> ExisteProductoAsync(int idProducto)
    {
        // Validación de la FK lógica: el producto pertenece a la base de Catálogo;
        // su existencia se comprueba por HTTP porque no hay una FK entre las bases.
        try
        {
            using var respuesta = await _cliente.GetAsync($"api/productos/{idProducto}");
            return respuesta.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.NotFound => false,
                _ => throw new CatalogoNoDisponibleException("Catálogo devolvió una respuesta inesperada.")
            };
        }
        catch (HttpRequestException excepcion)
        {
            throw new CatalogoNoDisponibleException("No fue posible comunicarse con Catálogo.", excepcion);
        }
        catch (TaskCanceledException excepcion)
        {
            throw new CatalogoNoDisponibleException("Se agotó el tiempo de espera de Catálogo.", excepcion);
        }
    }
}
