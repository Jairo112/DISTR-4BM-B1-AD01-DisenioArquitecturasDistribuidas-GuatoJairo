# Bases de datos de la entrega

Esta carpeta contiene el esquema completo de las tres bases de datos, generado
desde las migraciones de Entity Framework Core del proyecto. Cada servicio
conserva su base independiente y sus claves foráneas físicas internas.

Desde la raíz del repositorio, para SQL Server local con autenticación de Windows:

```powershell
sqlcmd -S localhost -E -C -b -i sql/crear-todo.sql
```

En SSMS también se pueden ejecutar, en este orden, `00-crear-bases.sql`,
`Catalogo.sql`, `Inventario.sql` y `Pedidos.sql`. Los scripts de esquema son
idempotentes: revisan el historial de migraciones antes de aplicar cada cambio.

Para cargar los datos ficticios de demostración, de forma opcional:

```powershell
sqlcmd -S localhost -E -C -I -b -i docs/datos-prueba.sql
```

**Atención:** `docs/datos-prueba.sql` elimina y vuelve a cargar los datos de las
tres bases del deber. Úsalo únicamente para preparar el entorno de demostración.
Los scripts de esta carpeta solo crean bases y aplican el esquema pendiente.

Las conexiones están incluidas en `src/*/appsettings.json`. Usan `localhost`,
las bases con sufijo `_AD01` y autenticación de Windows; no contienen contraseñas.
Si SQL Server usa otra instancia, ajusta `Server` en los tres archivos JSON.
También puedes sobrescribir las conexiones con variables de entorno
`ConnectionStrings__CatalogoDB`, `ConnectionStrings__InventarioDB` y
`ConnectionStrings__PedidosDB`.

Se entregan scripts SQL y migraciones reproducibles. Los archivos físicos de
SQL Server (`.mdf`, `.ldf` y `.ndf`) se excluyen mediante `.gitignore`.
