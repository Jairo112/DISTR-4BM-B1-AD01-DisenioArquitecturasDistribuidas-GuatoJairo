# ORDEN 06 – Inventario.Api: endpoints, reglas de stock y validación con Catálogo

Responde SIEMPRE en español.

## CONTEXTO
Deber universitario AD01: 3 microservicios ASP.NET Core Web API (.NET 10) + SQL Server.
- Catalogo.Api (http 5101): TERMINADO. Expone `GET /api/productos/{id}` → 200 con el producto o 404 `{ mensaje }`. NO lo modifiques. Copia su estilo de controladores y DTOs (`src/Catalogo.Api/Controllers`, `src/Catalogo.Api/DTOs`): DbContext inyectado en el controlador, DTOs con DataAnnotations, errores como `new { mensaje = "..." }`, async/await.
- Inventario.Api (http 5102, InventarioDB_AD01) ← ESTA TAREA. Ya tiene Models, `Data/InventarioDbContext.cs` y migración aplicada (orden 05). NO cambies modelos ni migraciones.
Tu entorno NO puede conectarse a SQL Server: no ejecutes la API contra la base; el jefe hará las pruebas HTTP. Tú solo compila.

## TAREA
Solo en `src/Inventario.Api`:

1. **CatalogoClient** (`Clients/CatalogoClient.cs`), cliente HTTP tipado registrado con `builder.Services.AddHttpClient<CatalogoClient>(...)`:
   - BaseAddress leída de `appsettings.json` → `"Servicios": { "CatalogoUrl": "http://localhost:5101" }`. Timeout 5 segundos.
   - `Task<bool> ExisteProductoAsync(int idProducto)`: GET `api/productos/{id}`. 200 → true; 404 → false.
   - Si Catálogo no responde (HttpRequestException, timeout/TaskCanceledException) o devuelve otro código, lanza `CatalogoNoDisponibleException` (clase propia, en la misma carpeta).
   - En cada controlador que use el cliente, atrapa esa excepción y devuelve `StatusCode(503, new { mensaje = "El servicio de Catálogo no está disponible. Intente más tarde." })`. La API nunca debe caerse.
   - Comenta en español que esta es la validación de la FK lógica.

2. **DTOs** en `DTOs/` (respuesta `XxxDto` y entrada `XxxCrearDto`, con DataAnnotations: `[Required]`, `[StringLength]`, `[Range(1, int.MaxValue)]` para cantidades/ids, `[Range(0, int.MaxValue)]` para stock inicial, `[RegularExpression("^(ENTRADA|SALIDA)$")]` para Tipo). Nunca expongas entidades directamente.

3. **BodegasController** (`api/bodegas`): GET, GET {id}, POST, PUT {id}, DELETE {id}.
   - DELETE: si la bodega tiene existencias, movimientos o reservas → 400 "No se puede eliminar la bodega porque tiene existencias, movimientos o reservas registradas."

4. **ExistenciasController** (`api/existencias`): GET con filtros opcionales `?idProducto=&idBodega=`, GET {id}, POST.
   - POST (IdProducto, IdBodega, Stock inicial ≥ 0): bodega inexistente → 400; producto inexistente en Catálogo → 400 "El producto indicado no existe en Catálogo."; ya existe (IdProducto, IdBodega) → 400.
   - El stock NO se edita directamente: solo cambia con movimientos y reservas (no hay PUT ni DELETE).
   - El DTO de respuesta incluye `Stock`, `StockReservado` (suma de reservas ACTIVAS) y `StockDisponible` (Stock − StockReservado).

5. **MovimientosController** (`api/movimientos`): GET con filtros opcionales `?idProducto=&idBodega=&tipo=`, GET {id}, POST.
   - POST valida bodega (400) y producto en Catálogo (400 / 503), luego, DENTRO DE UNA TRANSACCIÓN (`await using var tx = await _db.Database.BeginTransactionAsync()`), registra el movimiento (Fecha = DateTime.Now) y actualiza la existencia:
     - ENTRADA: suma la cantidad; si no existe la existencia (IdProducto, IdBodega) la crea.
     - SALIDA: stock disponible = Stock − suma de reservas ACTIVAS de ese producto en esa bodega. Si no existe la existencia o el disponible < cantidad → 400 "Stock insuficiente. Disponible: X." y no se guarda nada. Si alcanza, resta.
   - Commit solo al final; ante cualquier error, no queda nada guardado.
   - Devuelve 201 con el movimiento creado.

6. **ReservasController** (`api/reservas`): GET con filtros opcionales `?idProducto=&idBodega=&estado=`, GET {id}, POST, PUT {id}/confirmar, PUT {id}/cancelar.
   - POST: valida bodega (400) y producto en Catálogo (400 / 503); si el stock disponible < cantidad → 400 "Stock insuficiente. Disponible: X."; si alcanza crea la reserva con Estado ACTIVA → 201.
   - PUT confirmar: 404 si no existe; 400 si no está ACTIVA ("Solo se puede confirmar una reserva ACTIVA."). En UNA transacción: Estado = CONFIRMADA, resta la cantidad al Stock de la existencia y registra un MovimientoInventario SALIDA con esa cantidad. Devuelve 200 con la reserva.
   - PUT cancelar: 404 si no existe; 400 si no está ACTIVA; Estado = CANCELADA (el stock no cambia). Devuelve 200.

7. Códigos: 200 (GET/PUT), 201 (POST, con `CreatedAtAction`), 204 (DELETE), 400, 404, 503. Todos los mensajes en español.
8. `dotnet build` en la raíz.

## CRITERIOS DE ACEPTACIÓN
- [ ] `dotnet build` con 0 errores y 0 advertencias.
- [ ] `appsettings.json` tiene `Servicios:CatalogoUrl` y se usa en `Program.cs`.
- [ ] El POST de movimientos y la confirmación de reservas usan transacción explícita.
- [ ] Ningún endpoint devuelve entidades EF directamente.

## PROHIBIDO
- Tocar `Catalogo.Api`, `Pedidos.Api`, `docs/`, `.gitignore`, la `.sln`, los Models o las Migrations de Inventario.
- Repositorios, servicios de negocio, interfaces, AutoMapper, MediatR, Polly, autenticación.
- Datos de prueba, `nuget.config` u otros archivos de configuración nuevos.
- git init, commits o push.

## AL TERMINAR
Explícame en español, archivo por archivo, qué hiciste. Explica también:
- Por qué uso una transacción en el POST de movimientos y en confirmar reserva.
- Qué es el stock disponible y por qué se restan las reservas ACTIVAS.
- Cómo funciona la validación de la FK lógica y por qué Catálogo apagado da 503 y no 500.
Muestra la salida de `dotnet build`.
