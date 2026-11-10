Implementado únicamente en `src/Pedidos.Api`. Compilación con **0 errores y 0 advertencias** usando `--no-restore`. No ejecuté la API ni accedí a SQL Server.

Cambios, archivo por archivo:

| Archivo | Implementación |
|---|---|
| `Program.cs` | Registra `CatalogoClient` con timeout de 5 segundos y la respuesta de validación en español, igual que Inventario. |
| `appsettings.json` | Agrega `Servicios:CatalogoUrl` con `http://localhost:5101`. |
| `Clients/CatalogoClient.cs` | Obtiene productos, devuelve `null` ante 404 y lanza la excepción ante fallos, timeout o respuestas inesperadas. Documenta la FK lógica. |
| `Clients/CatalogoNoDisponibleException.cs` | Excepción que el controlador convierte en 503 con el mensaje solicitado. |
| `DTOs/ProductoCatalogoDto.cs` | Recibe identificador, nombre, precio y estado desde Catálogo. |
| `DTOs/ClienteCrearDto.cs` | Valida nombres, cédula y correo; se utiliza en POST y PUT. |
| `DTOs/ClienteDto.cs` | Respuesta del cliente sin relaciones EF. |
| `DTOs/PedidoCrearDto.cs` | Recibe solamente cliente y detalles. |
| `DTOs/DetallePedidoCrearDto.cs` | Recibe producto y cantidad, ambos con rango positivo. |
| `DTOs/PedidoDto.cs` | Respuesta con cliente, fecha, total, estado y detalles. |
| `DTOs/DetallePedidoDto.cs` | Respuesta con precio unitario y subtotal. |
| `DTOs/PedidoEstadoDto.cs` | Valida los cuatro estados admitidos. |
| `Controllers/ClientesController.cs` | CRUD, validación de duplicados y bloqueo del borrado si existen pedidos. |
| `Controllers/PedidosController.cs` | Consultas con filtros e `Include`, creación validada contra Catálogo, transiciones y borrado exclusivo de pendientes. Guarda pedido y detalles con un solo `SaveChangesAsync`. |

Ningún endpoint devuelve entidades EF directamente. Los modelos, migraciones y archivos fuente de los otros servicios permanecen intactos.

El servidor **obtiene el precio desde Catálogo** y calcula `Total = Σ(Cantidad × PrecioUnitario)` para impedir que el consumidor manipule los importes. Los DTOs de creación no contienen precio ni total; el precio consultado queda guardado en cada detalle como referencia histórica.

El flujo de estados es:

```text
PENDIENTE ------> CONFIRMADO ------> ENTREGADO
    |                 |
    +----> ANULADO <---+
```

`ENTREGADO` y `ANULADO` son finales. Cualquier otra transición devuelve 400. Solo se puede eliminar un pedido `PENDIENTE`; sus detalles se eliminan por Cascade.

No se integra todavía con Inventario porque el alcance solicitado no contempla consultas de stock, reservas ni descuentos de existencias.

Ejecuté `dotnet build` desde la raíz, pero la restauración falló por acceso denegado al `NuGet.Config` del usuario. Sin crear configuración adicional, compilé con las dependencias existentes:

```text
> dotnet build --no-restore

Compilación correcta.
    0 Advertencia(s)
    0 Errores

Tiempo transcurrido 00:00:01.52
```