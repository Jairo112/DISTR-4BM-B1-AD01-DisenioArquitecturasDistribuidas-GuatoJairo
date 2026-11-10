Implementado en `src/Catalogo.Api`. **La solución compila con 0 errores y 0 advertencias.** La migración quedó creada; no pudo aplicarse por un error de cifrado de SQL Server.

Archivo por archivo:

- `Catalogo.Api.csproj`: agregué `Microsoft.EntityFrameworkCore.SqlServer` y `Microsoft.EntityFrameworkCore.Design` en versión [10.0.14](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/10.0.14).
- `Models/Categoria.cs`: campos solicitados y colección `Productos`.
- `Models/Marca.cs`: campos solicitados y colección `Productos`.
- `Models/Producto.cs`: campos solicitados y navegaciones `Categoria` y `Marca`.
- `Data/CatalogoDbContext.cs`: agregué los tres DbSet y la configuración comentada en español: tablas singulares, identidades, longitudes, campos requeridos, precisión, default SQL, índice único, relaciones y CHECK.
- `appsettings.json`: agregué exactamente la conexión indicada, con autenticación de Windows y sin usuario ni contraseña.
- `Program.cs`: añadí únicamente los `using` necesarios y el registro solicitado del DbContext.
- `Migrations/20261105163934_Inicial.cs`: crea las tres tablas, ambas FK con `ReferentialAction.Restrict`, SKU único, dos CHECK, precio `decimal(18,2)` y default SQL `'ACTIVO'`.
- `Migrations/20261105163934_Inicial.Designer.cs`: metadatos generados por EF para la migración.
- `Migrations/CatalogoDbContextModelSnapshot.cs`: snapshot inicial del modelo.

No agregué datos semilla, controladores, DTOs ni capas adicionales. Los cambios de código están limitados a `Catalogo.Api`.

**Restrict** impide eliminar una categoría o marca mientras tenga productos asociados. Con `Cascade`, esa eliminación también borraría sus productos.

**SKU único** evita registrar dos productos con el mismo código. El **CHECK de Precio** rechaza valores cero o negativos directamente en la base de datos.

Estas son las salidas relevantes, omitiendo rutas repetidas y la traza interna:

```text
> dotnet ef migrations add Inicial --project src/Catalogo.Api

Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

Durante la creación apareció `NU1900` porque no pudo consultarse la auditoría de vulnerabilidades de NuGet. Resolví la restauración con los paquetes disponibles en caché y una configuración temporal fuera del repositorio; no añadí configuración nueva al proyecto.

```text
> dotnet ef database update --project src/Catalogo.Api

Build started...
Build succeeded.
System.InvalidOperationException: An exception has been raised that is likely due to a transient failure. Consider enabling transient error resiliency by adding 'EnableRetryOnFailure' to the 'UseSqlServer' call.
 ---> Microsoft.Data.SqlClient.SqlException (0x80131904): La instancia de SQL Server con la que intenta conectar requiere cifrado, pero este equipo no lo admite.

Error Number:20,State:0,Class:20
```

Conservé la migración y la cadena de conexión sin forzar ni reintentar la actualización. **La existencia de CatalogoDB y sus tablas queda sin verificar.**

```text
> dotnet build

Compilación correcta.
    0 Advertencia(s)
    0 Errores

Tiempo transcurrido 00:00:02.99
```