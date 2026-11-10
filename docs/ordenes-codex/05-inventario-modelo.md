# ORDEN 05 – Inventario.Api: modelo y base de datos

Responde SIEMPRE en español.

## CONTEXTO
Deber universitario AD01 (Aplicaciones Distribuidas): 3 microservicios REST independientes en ASP.NET Core Web API (.NET 10) con SQL Server, cada uno con su propia base:
- Catalogo.Api (http 5101, CatalogoDB_AD01): YA TERMINADO. Úsalo como referencia de estilo (`src/Catalogo.Api/Data/CatalogoDbContext.cs`, `Models/`, `.csproj`). NO lo modifiques.
- Inventario.Api (http 5102, InventarioDB_AD01) ← ESTA TAREA
- Pedidos.Api: NO se toca.
Arquitectura simple: Controlador + DbContext + DTOs + DataAnnotations. Nombres en español. Raíz del repo = carpeta actual. `dotnet ef` 10.0.14 instalado globalmente.
Tu entorno NO puede conectarse a SQL Server: NO ejecutes `dotnet ef database update`; el jefe la aplicará. Solo crea la migración y compila.

## TAREA
Solo en `src/Inventario.Api`:

1. Paquetes `Microsoft.EntityFrameworkCore.SqlServer` y `Microsoft.EntityFrameworkCore.Design` en la MISMA versión que usa `Catalogo.Api.csproj`.
2. Entidades en `src/Inventario.Api/Models/` (propiedades PascalCase):
   - `Bodega`: IdBodega (int), Nombre (string), Ciudad (string), Direccion (string?), colecciones `Existencias`, `Movimientos`, `Reservas`.
   - `Existencia`: IdExistencia (int), IdProducto (int), IdBodega (int), Stock (int), navegación `Bodega`.
   - `MovimientoInventario`: IdMovimiento (int), IdProducto (int), IdBodega (int), Tipo (string), Cantidad (int), Fecha (DateTime), navegación `Bodega`.
   - `ReservaStock`: IdReserva (int), IdProducto (int), IdBodega (int), Cantidad (int), Estado (string), navegación `Bodega`.
   - En cada entidad que tiene IdProducto agrega un comentario en español: "FK LÓGICA: IdProducto apunta a Producto en CatalogoDB_AD01 (otro microservicio, otra base). No hay FK en SQL; se valida llamando a Catalogo.Api por HTTP."
3. `src/Inventario.Api/Data/InventarioDbContext.cs` con DbSet `Bodegas`, `Existencias`, `Movimientos`, `Reservas` y Fluent API en `OnModelCreating`:
   - Tablas con nombre singular: `Bodega`, `Existencia`, `MovimientoInventario`, `ReservaStock`.
   - PK con identidad.
   - Longitudes: Bodega.Nombre 100 requerido, Ciudad 80 requerido, Direccion 200; MovimientoInventario.Tipo 10 requerido; ReservaStock.Estado 12 requerido.
   - FK FÍSICA Existencia→Bodega, MovimientoInventario→Bodega, ReservaStock→Bodega, todas con `DeleteBehavior.Restrict`.
   - IdProducto SIN relación (sin FK). Solo un índice normal en IdProducto de MovimientoInventario y ReservaStock.
   - Índice ÚNICO `(IdProducto, IdBodega)` en Existencia.
   - CHECK con `ToTable(t => t.HasCheckConstraint(...))`:
     - `CK_Existencia_Stock`: Stock >= 0
     - `CK_Movimiento_Cantidad`: Cantidad > 0
     - `CK_Movimiento_Tipo`: Tipo IN ('ENTRADA','SALIDA')
     - `CK_Reserva_Cantidad`: Cantidad > 0
     - `CK_Reserva_Estado`: Estado IN ('ACTIVA','CONFIRMADA','CANCELADA')
   - Default SQL: ReservaStock.Estado `'ACTIVA'`; MovimientoInventario.Fecha `GETDATE()`.
   - Comenta brevemente en español cada bloque.
4. `appsettings.json`: agrega
   ```json
   "ConnectionStrings": {
     "InventarioDB": "Server=localhost;Database=InventarioDB_AD01;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
5. `Program.cs`: registra `AddDbContext<InventarioDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("InventarioDB")))`. No cambies nada más.
6. Desde la raíz: `dotnet ef migrations add Inicial --project src/Inventario.Api`.
7. `dotnet build` en la raíz.

## CRITERIOS DE ACEPTACIÓN
- [ ] `dotnet build` con 0 errores y 0 advertencias.
- [ ] Migración `Inicial` + snapshot en `src/Inventario.Api/Migrations/`.
- [ ] La migración crea 4 tablas, exactamente 3 FK (todas hacia Bodega, `ReferentialAction.Restrict`), ninguna FK sobre IdProducto, índice único (IdProducto, IdBodega), 5 CHECK y los 2 defaults.
- [ ] `appsettings.json` sin `User Id` ni `Password`.

## PROHIBIDO
- Tocar `Catalogo.Api`, `Pedidos.Api`, `docs/`, `.gitignore` o la `.sln`.
- Controladores, DTOs o CatalogoClient (van en la orden 06).
- Repositorios, servicios, capas extra, AutoMapper, autenticación, datos semilla.
- `nuget.config` u otros archivos de configuración nuevos en el repo.
- git init, commits o push.

## AL TERMINAR
Explícame en español, archivo por archivo, qué hiciste. Incluye un dibujo en texto (ASCII) de cómo se relacionan las tablas, marcando cuál es FK física y dónde está la FK lógica hacia Catálogo. Explica por qué IdProducto no tiene FK y para qué sirve el índice único. Muestra la salida de `dotnet build` y de `dotnet ef migrations add`.
