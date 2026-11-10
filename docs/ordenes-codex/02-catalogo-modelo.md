# ORDEN 02 – Catalogo.Api: modelo y base de datos

Responde SIEMPRE en español.

## CONTEXTO
Deber universitario AD01 (Aplicaciones Distribuidas): 3 microservicios REST independientes en ASP.NET Core Web API (.NET 10) con SQL Server, cada uno con su propia base:
- Catalogo.Api (http 5101, CatalogoDB): Categoria, Marca, Producto.  ← ESTA TAREA
- Inventario.Api (http 5102, InventarioDB) y Pedidos.Api (http 5103, PedidosDB): NO se tocan ahora.
Sin gateway, login ni frontend. Arquitectura simple: Controlador + DbContext + DTOs + DataAnnotations. Nombres en español.
La solución ya existe y compila (orden 01). Raíz del repo = carpeta actual. Herramienta `dotnet ef` 10.0.14 ya instalada globalmente.

## TAREA
Solo en `src/Catalogo.Api`:

1. Paquetes (versión 10.x, la misma mayor que .NET):
   - `Microsoft.EntityFrameworkCore.SqlServer`
   - `Microsoft.EntityFrameworkCore.Design`
2. Entidades en `src/Catalogo.Api/Models/` (exactamente estos campos, propiedades en PascalCase):
   - `Categoria`: IdCategoria (int), Nombre (string), Descripcion (string?), colección `Productos`.
   - `Marca`: IdMarca (int), Nombre (string), PaisOrigen (string?), colección `Productos`.
   - `Producto`: IdProducto (int), IdCategoria (int), IdMarca (int), Nombre (string), SKU (string), Precio (decimal), Estado (string), navegaciones `Categoria` y `Marca`.
3. `src/Catalogo.Api/Data/CatalogoDbContext.cs` con DbSet `Categorias`, `Marcas`, `Productos` y configuración con Fluent API en `OnModelCreating`:
   - Tablas con nombre singular: `Categoria`, `Marca`, `Producto`.
   - PK `IdCategoria`, `IdMarca`, `IdProducto` con identidad.
   - Longitudes: Categoria.Nombre 100 requerido, Descripcion 250; Marca.Nombre 100 requerido, PaisOrigen 60; Producto.Nombre 150 requerido, SKU 30 requerido, Estado 10 requerido.
   - Producto.Precio `decimal(18,2)`.
   - Producto.Estado con valor por defecto `'ACTIVO'` en SQL.
   - Índice ÚNICO en Producto.SKU.
   - FK física Producto→Categoria (IdCategoria) y Producto→Marca (IdMarca) con `DeleteBehavior.Restrict`.
   - Restricciones CHECK: `CK_Producto_Precio` (Precio > 0) y `CK_Producto_Estado` (Estado IN ('ACTIVO','INACTIVO')). Usa `ToTable(t => t.HasCheckConstraint(...))`.
   - Comenta brevemente en español cada bloque de configuración.
4. `appsettings.json`: agrega
   ```json
   "ConnectionStrings": {
     "CatalogoDB": "Server=localhost;Database=CatalogoDB;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
5. `Program.cs`: registra el DbContext con `AddDbContext<CatalogoDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("CatalogoDB")))`. No cambies nada más de Program.cs.
6. Migración inicial (desde la raíz):
   `dotnet ef migrations add Inicial --project src/Catalogo.Api`
   y luego `dotnet ef database update --project src/Catalogo.Api`.
   Si `database update` falla por restricciones de tu entorno (sandbox, red, SQL Server), NO lo fuerces ni cambies la cadena de conexión: deja la migración creada y repórtame el error exacto.
7. `dotnet build` en la raíz.

## REGLAS
- EF Core Code First con migraciones. Autenticación de Windows, sin usuario ni contraseña.
- Restrict en ambas FK (no se puede borrar una categoría o marca que tenga productos).
- Estados en mayúsculas: ACTIVO / INACTIVO.
- Sin datos semilla (HasData) por ahora.

## CRITERIOS DE ACEPTACIÓN
- [ ] `dotnet build` con 0 errores y 0 advertencias.
- [ ] Existe `src/Catalogo.Api/Migrations/` con la migración `Inicial` y el snapshot.
- [ ] La migración crea 3 tablas, 2 FK con `onDelete: ReferentialAction.Restrict`, índice único en SKU, 2 CHECK y el default `'ACTIVO'`.
- [ ] Precio aparece como `decimal(18,2)` en la migración.
- [ ] `appsettings.json` no contiene `User Id` ni `Password`.
- [ ] (Si tu entorno lo permite) la base CatalogoDB existe con las tablas.

## PROHIBIDO
- Tocar `Inventario.Api`, `Pedidos.Api`, `docs/`, `.gitignore` o la `.sln`.
- Crear controladores o DTOs (van en la orden 03).
- Repositorios, servicios, interfaces, capas extra, AutoMapper, autenticación.
- Datos de prueba, `nuget.config` u otros archivos de configuración nuevos en el repo.
- git init, commits o push.

## AL TERMINAR
Explícame en español, archivo por archivo, qué hiciste. Explica además:
- Por qué Restrict y no Cascade en las FK de Producto.
- Por qué SKU es único y para qué sirve el CHECK de Precio.
Muestra la salida de `dotnet build` y de los comandos `dotnet ef`.
