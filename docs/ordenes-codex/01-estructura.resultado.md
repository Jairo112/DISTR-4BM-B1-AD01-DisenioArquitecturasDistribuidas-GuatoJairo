Esqueleto creado y verificado con **SDK 10.0.401**, **0 errores y 0 advertencias**. `docs/` quedó intacta.

Archivos creados o modificados:

| Archivo | Contenido y propósito |
|---|---|
| `DISTR-11PM-B1-AD01-DisenioArquitucturasDistribuidas-GuatoJairo.sln` | Solución clásica con los tres proyectos. |
| `README.md` | Título solicitado, autor Jairo Guato, paralelo 11PM y tabla de servicios, puertos y bases. |
| `.gitignore` | Plantilla de .NET; excluye `[Bb]in/` y `[Oo]bj/`. |
| `postman/.gitkeep` | Archivo vacío para conservar la carpeta. |

En **cada proyecto** (`Catalogo.Api`, `Inventario.Api` y `Pedidos.Api`):

| Archivo | Contenido y propósito |
|---|---|
| `<Proyecto>.csproj` | Framework `net10.0` y Swashbuckle.AspNetCore **10.3.1**, compatible con .NET 10. [NuGet](https://www-0.nuget.org/packages/swashbuckle.aspnetcore/) |
| `Program.cs` | Exactamente el código mínimo solicitado: controladores y Swagger únicamente en Development. |
| `Properties/launchSettings.json` | Único perfil `http`, navegador habilitado, ruta `swagger` y puerto 5101, 5102 o 5103 según el servicio. |
| `appsettings.json` | Configuración básica de registro generada por la plantilla. |
| `appsettings.Development.json` | Configuración de registro para desarrollo generada por la plantilla. |

Eliminé los dos archivos WeatherForecast y el archivo `.http` de cada proyecto. También quité `Microsoft.AspNetCore.OpenApi`.

Salida de `dotnet build`:
```text
  Determinando los proyectos que se van a restaurar...
  Todos los proyectos están actualizados para la restauración.

Compilación correcta.
    0 Advertencia(s)
    0 Errores

Tiempo transcurrido 00:00:05.86
```

Salida de `dotnet sln list`:
```text
Proyectos
---------
src\Catalogo.Api\Catalogo.Api.csproj
src\Inventario.Api\Inventario.Api.csproj
src\Pedidos.Api\Pedidos.Api.csproj
```

Verificación de ejecución:

| Servicio | Swagger | Documento JSON |
|---|---|---|
| Catalogo.Api — 5101 | HTTP 200 | HTTP 200, sin endpoints |
| Inventario.Api — 5102 | HTTP 200 | HTTP 200, sin endpoints |
| Pedidos.Api — 5103 | HTTP 200 | HTTP 200, sin endpoints |

La búsqueda con `rg` no encontró referencias a WeatherForecast. Detuve los tres procesos y confirmé que los puertos quedaron cerrados.

Para superar las restricciones de acceso y TLS de este entorno, utilicé configuración, caché y paquetes de NuGet en carpetas temporales, sin añadir configuración extra al repositorio.