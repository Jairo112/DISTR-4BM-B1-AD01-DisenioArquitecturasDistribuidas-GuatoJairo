# ORDEN 08 – Pedidos.Api: endpoints y reglas del pedido

Responde SIEMPRE en español.

## CONTEXTO
Deber universitario AD01: 3 microservicios ASP.NET Core Web API (.NET 10) + SQL Server.
- Catalogo.Api (5101): TERMINADO. `GET /api/productos/{id}` → 200 con JSON `{ idProducto, nombre, sku, precio, estado, ... }` (estado = "ACTIVO" | "INACTIVO") o 404.
- Inventario.Api (5102): TERMINADO. Es tu REFERENCIA de estilo: copia el patrón de `src/Inventario.Api/Clients/` (CatalogoClient + CatalogoNoDisponibleException), `Program.cs` (AddHttpClient con `Servicios:CatalogoUrl`, timeout 5 s, `InvalidModelStateResponseFactory` con `mensaje` en español), `DTOs/` y `Controllers/`. NO lo modifiques.
- Pedidos.Api (5103, PedidosDB_AD01) ← ESTA TAREA. Ya tiene Models, `Data/PedidosDbContext.cs` y migración aplicada (orden 07). NO cambies modelos ni migraciones.
Tu entorno NO puede conectarse a SQL Server: no ejecutes la API contra la base; el jefe hará las pruebas HTTP. Solo compila.

## TAREA
Solo en `src/Pedidos.Api`:

1. **CatalogoClient** (`Clients/`), tipado igual que en Inventario:
   - `appsettings.json` → `"Servicios": { "CatalogoUrl": "http://localhost:5101" }`; registrar en `Program.cs` con timeout 5 s.
   - `Task<ProductoCatalogoDto?> ObtenerProductoAsync(int idProducto)`: 200 → objeto con IdProducto, Nombre, Precio, Estado; 404 → null; caída/timeout/otro código → `CatalogoNoDisponibleException`.
   - Los controladores atrapan la excepción y devuelven `StatusCode(503, new { mensaje = "El servicio de Catálogo no está disponible. Intente más tarde." })`.
   - Comenta en español que es la validación de la FK lógica.
   - En `Program.cs` agrega también el mismo `InvalidModelStateResponseFactory` que Inventario.

2. **DTOs** en `DTOs/` con DataAnnotations. Nunca expongas entidades.
   - Cliente: `[Required]`, `[StringLength(150)]`, Cedula `[RegularExpression("^[0-9]{10}$", ErrorMessage = "La cédula debe tener 10 dígitos numéricos.")]`, Correo `[EmailAddress]` + `[StringLength(150)]`.
   - Pedido crear: `{ idCliente, detalles: [ { idProducto, cantidad } ] }` con `[Range(1, int.MaxValue)]` en ids y cantidad. NO incluye precio ni total.
   - PedidoDto de respuesta incluye IdPedido, IdCliente, NombreCliente, Fecha, Total, Estado y la lista de detalles (IdDetalle, IdProducto, Cantidad, PrecioUnitario, Subtotal).
   - Cambio de estado: `{ estado }` con `[RegularExpression("^(PENDIENTE|CONFIRMADO|ENTREGADO|ANULADO)$")]`.

3. **ClientesController** (`api/clientes`): GET, GET {id}, POST, PUT {id}, DELETE {id}.
   - Cédula repetida → 400 "Ya existe un cliente con esa cédula."; correo repetido → 400 "Ya existe un cliente con ese correo." (en POST y PUT, excluyendo al mismo cliente en PUT).
   - DELETE con pedidos → 400 "No se puede eliminar el cliente porque tiene pedidos registrados."

4. **PedidosController** (`api/pedidos`):
   - GET con filtros opcionales `?idCliente=&estado=`; GET {id} con detalles (`Include`). 404 si no existe.
   - POST:
     * cliente inexistente → 400; sin detalles (null o vacío) → 400 "El pedido debe tener al menos un detalle."; cantidad ≤ 0 → 400.
     * por cada detalle llama a `ObtenerProductoAsync`: null → 400 "El producto {id} no existe en Catálogo."; Estado ≠ "ACTIVO" → 400 "El producto {id} está INACTIVO y no se puede pedir."; Catálogo caído → 503.
     * PrecioUnitario = precio de Catálogo (el cliente nunca lo envía). Total = Σ Cantidad × PrecioUnitario, calculado en el servidor.
     * Guarda Pedido + Detalles en un solo `SaveChangesAsync` (Estado "PENDIENTE", Fecha DateTime.Now) → 201 con `CreatedAtAction`.
   - PUT {id}/estado: 404 si no existe. Transiciones permitidas (cualquier otra → 400 "No se puede cambiar el estado de X a Y."):
     * PENDIENTE → CONFIRMADO
     * CONFIRMADO → ENTREGADO
     * PENDIENTE → ANULADO, CONFIRMADO → ANULADO
     Devuelve 200 con el pedido.
   - DELETE {id}: 404 si no existe; si no está PENDIENTE → 400 "Solo se puede eliminar un pedido PENDIENTE."; si lo está, lo borra (los detalles caen por Cascade) → 204.
   - NO integres con Inventario (el enunciado no lo pide).

5. Códigos: 200, 201, 204, 400, 404, 503. Mensajes en español como `new { mensaje = "..." }`. async/await.
6. `dotnet build` en la raíz.

## CRITERIOS DE ACEPTACIÓN
- [ ] `dotnet build` con 0 errores y 0 advertencias.
- [ ] El cliente del API no puede enviar precio ni total.
- [ ] Ningún endpoint devuelve entidades EF directamente (cuidado con ciclos Pedido↔Cliente).

## PROHIBIDO
- Tocar `Catalogo.Api`, `Inventario.Api`, `docs/`, `.gitignore`, la `.sln`, los Models o las Migrations de Pedidos.
- Repositorios, servicios de negocio, interfaces, AutoMapper, MediatR, Polly, autenticación.
- Datos de prueba, `nuget.config` u otros archivos de configuración nuevos.
- git init, commits o push.

## AL TERMINAR
Explícame en español, archivo por archivo, qué hiciste. Explica también:
- Por qué el precio y el total los calcula el servidor y no el cliente.
- Cómo funciona el flujo de estados del pedido (dibujo ASCII).
- Por qué no se integra todavía con Inventario.
Muestra la salida de `dotnet build`.
