using System.Net;
using System.Text.Json;
using Pedidos.Api.DTOs;

namespace Pedidos.Api.Clients;

public class CatalogoClient
{
    private readonly HttpClient _cliente;

    public CatalogoClient(HttpClient cliente)
    {
        _cliente = cliente;
    }

    public async Task<ProductoCatalogoDto?> ObtenerProductoAsync(int idProducto)
    {
        // Validación de la FK lógica: el producto pertenece a la base de Catálogo;
        // se consulta por HTTP porque no hay una FK entre las bases de los servicios.
        try
        {
            using var respuesta = await _cliente.GetAsync($"api/productos/{idProducto}");
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (respuesta.StatusCode != HttpStatusCode.OK)
            {
                throw new CatalogoNoDisponibleException("Catálogo devolvió una respuesta inesperada.");
            }

            return await respuesta.Content.ReadFromJsonAsync<ProductoCatalogoDto>()
                ?? throw new CatalogoNoDisponibleException("Catálogo devolvió una respuesta vacía.");
        }
        catch (HttpRequestException excepcion)
        {
            throw new CatalogoNoDisponibleException("No fue posible comunicarse con Catálogo.", excepcion);
        }
        catch (TaskCanceledException excepcion)
        {
            throw new CatalogoNoDisponibleException("Se agotó el tiempo de espera de Catálogo.", excepcion);
        }
        catch (JsonException excepcion)
        {
            throw new CatalogoNoDisponibleException("Catálogo devolvió una respuesta inválida.", excepcion);
        }
    }
}
