using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Catalogo.Api.Filtros;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Catalogo.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CatalogoDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("CatalogoDB")));
builder.Services.AddControllers(o => o.Filters.Add<MensajeErrorFilter>()).AddJsonOptions(opciones =>
{
    opciones.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
}).ConfigureApiBehaviorOptions(opciones =>
{
    opciones.InvalidModelStateResponseFactory = contexto =>
    {
        var errores = new Dictionary<string, List<string>>();
        foreach (var entrada in contexto.ModelState)
        {
            // Un fallo del lector puede añadir también "datos is required".
            // Se conserva la causa original y se evita ese segundo error del parámetro.
            if (entrada.Key.Equals("datos", StringComparison.OrdinalIgnoreCase)
                && contexto.ModelState.Keys.Any(clave => clave.StartsWith('$')))
            {
                continue;
            }

            // Se eliminan los prefijos del lector JSON y del parámetro del controlador.
            // Las rutas anidadas conservan sus índices, por ejemplo detalles[0].cantidad.
            var ruta = entrada.Key;
            if (ruta.StartsWith("$.", StringComparison.Ordinal))
            {
                ruta = ruta[2..];
            }
            if (ruta.StartsWith("datos.", StringComparison.OrdinalIgnoreCase))
            {
                ruta = ruta[6..];
            }
            var campo = string.IsNullOrEmpty(ruta) || ruta == "$"
                || ruta.Equals("datos", StringComparison.OrdinalIgnoreCase)
                ? "cuerpo"
                : string.Join(".", ruta.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));

            foreach (var error in entrada.Value.Errors)
            {
                var texto = error.ErrorMessage.Trim();
                // Los mensajes propios de las anotaciones están en español.
                // Los del lector JSON o del enlace de parámetros nunca se exponen en inglés.
                var errorDeFormato = error.Exception is not null
                    || texto.Contains("JSON", StringComparison.OrdinalIgnoreCase)
                    || texto.Contains("could not be converted", StringComparison.OrdinalIgnoreCase)
                    || texto.Contains("is required", StringComparison.OrdinalIgnoreCase)
                    || texto.Contains("body", StringComparison.OrdinalIgnoreCase)
                    || !Regex.IsMatch(texto, @"^(El|La|Los|Las|No|Debe|Ingrese|Seleccione|Indique|Verifique)\b",
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                var detalleError = error.Exception?.GetBaseException().Message ?? texto;
                var campoNoPermitido = detalleError.Contains(
                    "could not be mapped to any .NET member", StringComparison.OrdinalIgnoreCase);
                if (campoNoPermitido)
                {
                    var parametroCuerpo = contexto.ActionDescriptor.Parameters
                        .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body);
                    var opcionesJson = contexto.HttpContext.RequestServices
                        .GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
                    var tipoObjeto = parametroCuerpo?.ParameterType;
                    foreach (var segmento in ruta.Split('.').SkipLast(1))
                    {
                        if (tipoObjeto is null) break;
                        var nombrePropiedad = Regex.Replace(segmento, @"\[\d+\]", "");
                        var propiedad = opcionesJson.GetTypeInfo(tipoObjeto).Properties
                            .FirstOrDefault(p => p.Name.Equals(nombrePropiedad,
                                StringComparison.OrdinalIgnoreCase));
                        tipoObjeto = propiedad?.PropertyType;
                        if (tipoObjeto is null) break;
                        var coleccion = tipoObjeto.GetInterfaces().Append(tipoObjeto)
                            .FirstOrDefault(t => t.IsGenericType
                                && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                        if (coleccion is not null && tipoObjeto != typeof(string))
                        {
                            tipoObjeto = coleccion.GetGenericArguments()[0];
                        }
                    }
                    var camposPermitidos = tipoObjeto is null ? "" : string.Join(", ",
                        opcionesJson.GetTypeInfo(tipoObjeto).Properties
                            .Where(p => p.Set is not null)
                            .Select(p => p.Name));
                    texto = $"El campo {campo} no está permitido en este objeto.";
                    if (!string.IsNullOrEmpty(camposPermitidos))
                    {
                        texto += $" Campos permitidos: {camposPermitidos}.";
                    }
                }
                else if (errorDeFormato)
                {
                    texto = campo == "cuerpo"
                        ? "No se enviaron datos o el JSON está mal formado."
                        : $"El campo {campo} tiene un formato inválido.";
                }
                if (!texto.EndsWith('.'))
                {
                    texto += ".";
                }

                if (!errores.TryGetValue(campo, out var mensajes))
                {
                    mensajes = new List<string>();
                    errores[campo] = mensajes;
                }
                if (!mensajes.Contains(texto))
                {
                    mensajes.Add(texto);
                }
            }
        }

        // La fábrica reúne los errores en un mensaje; el filtro añade la acción y el método HTTP.
        return new BadRequestObjectResult(new
        {
            mensaje = string.Join(" ", errores.Values.SelectMany(mensajes => mensajes))
        });
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.Run();
