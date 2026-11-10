using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Pedidos.Api.Filtros;

public class MensajeErrorFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext contexto, ResultExecutionDelegate siguiente)
    {
        // Se ejecuta antes de escribir la respuesta, incluidos los rechazos automáticos de ModelState.
        // Solo transforma errores con mensaje; los códigos HTTP y las respuestas exitosas se conservan.
        if (contexto.Result is ObjectResult resultado
            && (resultado.StatusCode ?? contexto.HttpContext.Response.StatusCode) >= 400
            && resultado.Value?.GetType().GetProperty("mensaje")?.GetValue(resultado.Value) is string mensaje)
        {
            var metodo = contexto.HttpContext.Request.Method.ToUpperInvariant();
            var accion = metodo switch
            {
                "GET" => "CONSULTAR",
                "POST" => "REGISTRAR",
                "PUT" => "ACTUALIZAR",
                "PATCH" => "ACTUALIZAR",
                "DELETE" => "ELIMINAR",
                _ => "PROCESAR"
            };
            var prefijo = $"ERROR AL {accion} ({metodo}): ";
            // Evita repetir el prefijo si la respuesta ya fue formateada.
            if (!mensaje.StartsWith(prefijo, StringComparison.Ordinal))
            {
                mensaje = prefijo + mensaje;
            }

            resultado.Value = new { mensaje };
            resultado.DeclaredType = null;
        }

        await siguiente();
    }
}
