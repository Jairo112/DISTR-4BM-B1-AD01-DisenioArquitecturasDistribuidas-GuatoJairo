Implementado en `src/Inventario.Api`. La compilación final terminó con **0 errores y 0 advertencias**. No modifiqué fuentes de `Catalogo.Api`, `Pedidos.Api`, `docs/`, `.gitignore` ni la solución. No ejecuté `database update`.

Archivo por archivo:

- `Inventario.Api.csproj`: agregué EF Core SqlServer y Design **10.0.14**, igual que Catálogo. También fijé `System.Security.Cryptography.Pkcs` en **10.0.14**, siguiendo su referencia.
- `Models/Bodega.cs`: identidad, datos de ubicación y las tres colecciones de navegación.
- `Models/Existencia.cs`: producto, bodega, stock y navegación `Bodega`; incluye el comentario de FK lógica solicitado.
- `Models/MovimientoInventario.cs`: producto, bodega, tipo, cantidad y fecha; incluye navegación y comentario.
- `Models/ReservaStock.cs`: producto, bodega, cantidad y estado inicial `ACTIVA`; incluye navegación y comentario.
- `Data/InventarioDbContext.cs`: cuatro DbSet, tablas singulares, PK con identidad, longitudes, campos requeridos, tres FK con `Restrict`, índices, cinco CHECK y defaults SQL `GETDATE()` y `'ACTIVA'`. Los bloques están comentados en español.
- `appsettings.json`: agregué la conexión `InventarioDB` con autenticación integrada, sin `User Id` ni `Password`.
- `Program.cs`: añadí únicamente los `using` necesarios y el registro de `InventarioDbContext`.
- `Migrations/20261105211253_Inicial.cs`: crea las cuatro tablas con sus restricciones e índices.
- `Migrations/20261105211253_Inicial.Designer.cs`: metadatos del modelo correspondiente a `Inicial`.
- `Migrations/InventarioDbContextModelSnapshot.cs`: snapshot generado. EF confirmó que coincide con el modelo actual.

Las relaciones quedan así:

```text
InventarioDB_AD01

Existencia.IdBodega -----------+
MovimientoInventario.IdBodega -+--> Bodega.IdBodega
ReservaStock.IdBodega ---------+
       3 FK FISICAS, todas con Restrict


Existencia.IdProducto -----------+
MovimientoInventario.IdProducto -+ ..> CatalogoDB_AD01.Producto.IdProducto
ReservaStock.IdProducto ---------+
       FK LOGICA: referencia entre microservicios, mediante HTTP
```

`IdProducto` no tiene FK SQL porque `Producto` pertenece a otra base y a otro microservicio. Su validación mediante HTTP corresponde a la siguiente orden; aquí quedó documentada, sin crear un cliente.

El índice único de `Existencia(IdProducto, IdBodega)` impide duplicar el saldo del mismo producto dentro de una bodega. Permite almacenar ese producto en bodegas diferentes.

Verifiqué en la migración: **4 tablas, 3 FK hacia Bodega con `Restrict`, ninguna FK sobre IdProducto, 5 CHECK, 2 defaults y el índice único compuesto**.

La ejecución que creó la migración:

```text
dotnet ef migrations add Inicial --project src/Inventario.Api

[Dos advertencias NU1900 por no poder consultar api.nuget.org]
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

Salida de la compilación final, con rutas abreviadas:

```text
dotnet build

  Determinando los proyectos que se van a restaurar...
  [Restauración completada]
  Pedidos.Api -> ...\Pedidos.Api.dll
  Inventario.Api -> ...\Inventario.Api.dll
  Catalogo.Api -> ...\Catalogo.Api.dll

Compilación correcta.
    0 Advertencia(s)
    0 Errores

Tiempo transcurrido 00:00:03.70
```

Para superar las restricciones del entorno, usé configuración temporal de NuGet fuera del repositorio y desactivé su auditoría **solo durante la ejecución final**. No dejé esa desactivación en el proyecto; la consulta de vulnerabilidades no pudo verificarse aquí.