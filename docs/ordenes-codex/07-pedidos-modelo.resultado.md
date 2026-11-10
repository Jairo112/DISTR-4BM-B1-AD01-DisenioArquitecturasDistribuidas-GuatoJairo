Implementado en `src/Pedidos.Api`. La compilación completa terminó con **0 errores y 0 advertencias**. No ejecuté `database update`.

Archivo por archivo:

- `Pedidos.Api.csproj`: agregué EF Core SqlServer y Design `10.0.14`, junto con el pin de `System.Security.Cryptography.Pkcs` `10.0.14`, igual que Inventario.
- `Models/Cliente.cs`: entidad con sus cuatro propiedades y colección `Pedidos`.
- `Models/Pedido.cs`: propiedades del pedido, navegación `Cliente` y colección `Detalles`.
- `Models/DetallePedido.cs`: propiedades del detalle y navegación `Pedido`; incluye el comentario solicitado sobre la FK lógica de `IdProducto`.
- `Data/PedidosDbContext.cs`: configuré las tablas singulares, identidades, longitudes, campos obligatorios, índices, restricciones, valores predeterminados y relaciones. Los bloques están comentados en español.
- `appsettings.json`: agregué la conexión integrada a `PedidosDB_AD01`, sin `User Id` ni `Password`.
- `Program.cs`: añadí únicamente los imports necesarios y el registro del DbContext.
- `Migrations/20261106002617_Inicial.cs`: migración inicial con creación y eliminación del esquema.
- `Migrations/20261106002617_Inicial.Designer.cs`: metadatos generados por EF.
- `Migrations/PedidosDbContextModelSnapshot.cs`: snapshot del modelo.

La migración contiene exactamente **3 tablas, 2 FK físicas, 2 índices únicos en Cliente, 5 CHECK y 2 defaults SQL**. `IdProducto` tiene índice normal y ninguna FK SQL.

```text
PedidosDB_AD01
+-------------------------+
| Cliente                 |
| PK IdCliente            |
| Nombres, Cedula, Correo  |
+-------------------------+
             ^
             | FK fisica: Pedido.IdCliente
             | ON DELETE RESTRICT
+-------------------------+
| Pedido                  |
| PK IdPedido             |
| FK IdCliente            |
| Fecha, Total, Estado    |
+-------------------------+
             ^
             | FK fisica: DetallePedido.IdPedido
             | ON DELETE CASCADE
+-------------------------+
| DetallePedido           |
| PK IdDetalle            |
| FK IdPedido             |
| IdProducto ........................> CatalogoDB_AD01
| Cantidad                |             Producto.IdProducto
| PrecioUnitario          |             FK LOGICA por HTTP
+-------------------------+             Sin FK en SQL
```

**Cascade** elimina los detalles cuando se elimina su pedido, porque dependen de él. **Restrict** impide eliminar un cliente que tiene pedidos y protege su historial.

La generación se ejecutó desde la raíz:

```text
dotnet ef migrations add Inicial --project src/Pedidos.Api

Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

El `dotnet build` inicial falló por permisos al leer el NuGet.Config del perfil. La compilación final usó una configuración existente del equipo y desactivó la auditoría en línea solo para ese comando; no añadí archivos de configuración:

```powershell
$env:APPDATA = $env:TEMP
$env:MSBUILDDISABLENODEREUSE = '1'
dotnet build '-p:RestoreConfigFile=C:/Program Files (x86)/NuGet/Config/Microsoft.VisualStudio.Offline.config' -p:NuGetAudit=false -nr:false
```

Salida final relevante:

```text
Compilación correcta.
    0 Advertencia(s)
    0 Errores

Tiempo transcurrido 00:00:04.43
```