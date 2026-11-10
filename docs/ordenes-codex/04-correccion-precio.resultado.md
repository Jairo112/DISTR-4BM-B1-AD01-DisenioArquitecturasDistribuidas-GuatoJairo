Modifiqué únicamente la línea del atributo `Range` de `Precio` en `ProductoCrearDto.cs`: agregué ambas opciones de cultura invariante y el mensaje `"El precio debe ser mayor que 0."`.

La cultura del equipo interpretaba los límites con su separador decimal local y fallaba al convertir `"0.01"`. Ahora se utiliza la cultura invariante.

`dotnet build` falló por acceso denegado a `NuGet.Config`. **`dotnet build --no-restore` terminó con 0 errores y 0 advertencias.**

Las pruebas HTTP quedan pendientes para el jefe.