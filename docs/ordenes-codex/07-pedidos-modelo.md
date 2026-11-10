# ORDEN 07 – Pedidos.Api: modelo y base de datos

Responde SIEMPRE en español.

## CONTEXTO
Deber universitario AD01: 3 microservicios ASP.NET Core Web API (.NET 10) + SQL Server, cada uno con su base:
- Catalogo.Api (5101) e Inventario.Api (5102): TERMINADOS. Úsalos como referencia de estilo (`src/Inventario.Api/Data/InventarioDbContext.cs`, `Models/`, `.csproj`). NO los modifiques.
- Pedidos.Api (http 5103, PedidosDB_AD01) ← ESTA TAREA.
Arquitectura simple: Controlador + DbContext + DTOs + DataAnnotations. Nombres en español. Raíz del repo = carpeta actual. `dotnet ef` 10.0.14 instalado globalmente.
Tu entorno NO puede conectarse a SQL Server: NO ejecutes `dotnet ef database update`; el jefe la aplicará. Solo crea la migración y compila.

## TAREA
Solo en `src/Pedidos.Api`:

1. Paquetes `Microsoft.EntityFrameworkCore.SqlServer` y `Microsoft.EntityFrameworkCore.Design` en la MISMA versión que `Inventario.Api.csproj` (incluye también el mismo pin de `System.Security.Cryptography.Pkcs` si Inventario lo tiene).
2. Entidades en `Models/` (PascalCase):
   - `Cliente`: IdCliente, Nombres (string), Cedula (string), Correo (string), colección `Pedidos`.
   - `Pedido`: IdPedido, IdCliente, Fecha (DateTime), Total (decimal), Estado (string), navegación `Cliente`, colección `Detalles`.
   - `DetallePedido`: IdDetalle, IdPedido, IdProducto (int), Cantidad (int), PrecioUnitario (decimal), navegación `Pedido`.
   - En `DetallePedido.IdProducto` comenta en español: "FK LÓGICA: IdProducto apunta a Producto en CatalogoDB_AD01 (otro microservicio, otra base). No hay FK en SQL; se valida llamando a Catalogo.Api por HTTP."
3. `Data/PedidosDbContext.cs` con DbSet `Clientes`, `Pedidos`, `Detalles` y Fluent API:
   - Tablas singulares: `Cliente`, `Pedido`, `DetallePedido`. PK con identidad.
   - Cliente: Nombres 150 requerido; Cedula `char(10)` requerido, índice ÚNICO; Correo 150 requerido, índice ÚNICO.
   - CHECK `CK_Cliente_Cedula`: `LEN([Cedula]) = 10 AND [Cedula] NOT LIKE '%[^0-9]%'` (solo 10 dígitos).
   - Pedido: Total `decimal(18,2)`; Estado 12 requerido, default SQL `'PENDIENTE'`; Fecha default SQL `GETDATE()`.
   - CHECK `CK_Pedido_Estado`: Estado IN ('PENDIENTE','CONFIRMADO','ENTREGADO','ANULADO'); `CK_Pedido_Total`: Total >= 0.
   - DetallePedido: PrecioUnitario `decimal(18,2)`; CHECK `CK_Detalle_Cantidad`: Cantidad > 0; `CK_Detalle_Precio`: PrecioUnitario >= 0. Índice normal en IdProducto.
   - FK FÍSICA Pedido→Cliente con `DeleteBehavior.Restrict`.
   - FK FÍSICA DetallePedido→Pedido con `DeleteBehavior.Cascade`.
   - IdProducto SIN relación (sin FK).
   - Comenta en español cada bloque, y en las FK explica por qué una es Restrict y la otra Cascade.
4. `appsettings.json`: agrega
   ```json
   "ConnectionStrings": {
     "PedidosDB": "Server=localhost;Database=PedidosDB_AD01;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
5. `Program.cs`: registra `AddDbContext<PedidosDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("PedidosDB")))`. No cambies nada más.
6. Desde la raíz: `dotnet ef migrations add Inicial --project src/Pedidos.Api`.
7. `dotnet build` en la raíz.

## CRITERIOS DE ACEPTACIÓN
- [ ] `dotnet build` con 0 errores y 0 advertencias.
- [ ] Migración `Inicial` + snapshot en `src/Pedidos.Api/Migrations/`.
- [ ] La migración crea 3 tablas, exactamente 2 FK (Pedido→Cliente Restrict, DetallePedido→Pedido Cascade), ninguna FK sobre IdProducto, 2 índices únicos en Cliente, 5 CHECK y los 2 defaults.
- [ ] `appsettings.json` sin `User Id` ni `Password`.

## PROHIBIDO
- Tocar `Catalogo.Api`, `Inventario.Api`, `docs/`, `.gitignore` o la `.sln`.
- Controladores, DTOs o CatalogoClient (van en la orden 08).
- Repositorios, servicios, capas extra, AutoMapper, autenticación, datos semilla.
- `nuget.config` u otros archivos de configuración nuevos.
- git init, commits o push.

## AL TERMINAR
Explícame en español, archivo por archivo, qué hiciste. Incluye un dibujo ASCII de las tablas marcando FK físicas (Restrict / Cascade) y la FK lógica hacia Catálogo. Explica por qué en DetallePedido se usa Cascade y en Cliente no. Muestra la salida de `dotnet build` y de `dotnet ef migrations add`.
