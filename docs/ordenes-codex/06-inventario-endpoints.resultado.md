Implementado únicamente en `src/Inventario.Api`. La solución compila con **0 errores y 0 advertencias** mediante `dotnet build --no-restore`. No inicié la API ni conecté con SQL Server.

Los cambios, archivo por archivo, dentro de [Inventario.Api](<C:/Users/ASUS TUF GAMING A16/Desktop/Materias Cuarto/Aplicaciones_Distribuidas/ProductosDeber/DISTR-11PM-B1-AD01-DisenioArquitucturasDistribuidas-GuatoJairo/src/Inventario.Api>):

| Archivo | Qué hice |
|---|---|
| `Program.cs` | Registré `AddHttpClient<CatalogoClient>`, URL desde configuración y timeout de 5 segundos. Configuré los errores de validación como 400 con mensaje en español. |
| `appsettings.json` | Añadí `Servicios:CatalogoUrl` con `http://localhost:5101`. |
| `Clients/CatalogoClient.cs` | Implementé la consulta del producto: 200 devuelve `true`, 404 devuelve `false`; errores de conexión, timeout y otros códigos lanzan la excepción propia. Comenté la FK lógica en español. |
| `Clients/CatalogoNoDisponibleException.cs` | Añadí la excepción que los controladores convierten en 503 con el mensaje solicitado. |
| `DTOs/BodegaCrearDto.cs` | Validé nombre y ciudad obligatorios y longitudes acordes con la base. |
| `DTOs/BodegaDto.cs` | Definí la respuesta de bodegas. |
| `DTOs/ExistenciaCrearDto.cs` | Validé identificadores positivos y stock inicial mayor o igual que cero. |
| `DTOs/ExistenciaDto.cs` | Incluí `Stock`, `StockReservado` y `StockDisponible`. |
| `DTOs/MovimientoCrearDto.cs` | Validé identificadores, cantidad positiva y tipo `ENTRADA` o `SALIDA`. |
| `DTOs/MovimientoDto.cs` | Definí la respuesta del movimiento, incluida su fecha. |
| `DTOs/ReservaCrearDto.cs` | Validé identificadores y cantidad positiva. |
| `DTOs/ReservaDto.cs` | Definí la respuesta de reservas y su estado. |
| `Controllers/BodegasController.cs` | Implementé GET, GET por id, POST, PUT y DELETE. El borrado devuelve 400 si hay registros relacionados y 204 si se completa. |
| `Controllers/ExistenciasController.cs` | Implementé GET con filtros, GET por id y POST con validación de bodega, producto y duplicados. Sin PUT ni DELETE. |
| `Controllers/MovimientosController.cs` | Implementé consultas con filtros y creación transaccional: entradas suman stock y salidas respetan las reservas ACTIVAS. |
| `Controllers/ReservasController.cs` | Implementé consultas, creación ACTIVA, confirmación con descuento y movimiento SALIDA, y cancelación sin modificar stock. |

Todos los endpoints devuelven DTOs o mensajes; ninguno devuelve entidades EF directamente. Los POST usan `CreatedAtAction`.

Las transacciones garantizan que **el movimiento y el cambio de stock se guarden juntos**. Al confirmar una reserva, también deben guardarse juntos el estado CONFIRMADA, el descuento de stock y el movimiento SALIDA. El commit ocurre al final; si hay un error previo, la transacción se revierte. Usé aislamiento serializable para proteger las comprobaciones frente a escrituras simultáneas.

El **stock disponible** es `Stock − suma de reservas ACTIVAS`. Esas unidades siguen físicamente en bodega, pero ya están comprometidas. Confirmar descuenta el stock físico y deja de contar la reserva como ACTIVA; cancelar libera las unidades sin cambiar el stock físico.

La **FK lógica** se valida consultando Catálogo por HTTP, porque el producto está en otra base de datos. Un 404 confirma que no existe y produce 400. Si Catálogo está apagado, la excepción se captura y devuelve **503**, indicando que una dependencia está temporalmente indisponible.

Ejecuté ambos comandos desde la raíz. `dotnet build` falló durante la restauración por acceso denegado al `NuGet.Config` del usuario:

```text
ERROR al compilar.
    0 Advertencia(s)
    3 Errores
```

Con las dependencias ya restauradas, `dotnet build --no-restore` terminó correctamente:

```text
Compilación correcta.
    0 Advertencia(s)
    0 Errores

Tiempo transcurrido 00:00:01.37
```

No modifiqué Catálogo, Pedidos, modelos, migraciones ni los demás archivos prohibidos.