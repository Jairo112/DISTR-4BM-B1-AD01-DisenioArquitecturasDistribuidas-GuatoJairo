# AD01 – Diseño de Arquitecturas Distribuidas

**Comercio y distribución multibodega con 3 microservicios REST**

Autor: Jairo Guato  
Paralelo: 4BM (Cuarto B Matutino) · Instituto Superior Tecnológico Cordillera · Aplicaciones Distribuidas

## Tecnologías
ASP.NET Core Web API (.NET 10) · Entity Framework Core 10 (Code First + migraciones) · SQL Server · Swagger · Postman

## Arquitectura
Tres microservicios **independientes**: cada uno tiene su propio proyecto, su propio puerto y **su propia base de datos**. Ningún servicio lee la base de otro; cuando necesitan datos ajenos, se los piden por HTTP.

```
              ┌──────────────────────┐
              │   Catalogo.Api 5101  │──── CatalogoDB_AD01
              │ Categoria·Marca·     │     (Categoria, Marca, Producto)
              │ Producto             │
              └──────────▲───────────┘
          GET /api/productos/{id}  (valida la FK lógica IdProducto)
            ┌────────────┴─────────────┐
┌───────────┴──────────┐   ┌───────────┴──────────┐
│ Inventario.Api 5102  │   │  Pedidos.Api 5103    │
└───────────┬──────────┘   └───────────┬──────────┘
   InventarioDB_AD01             PedidosDB_AD01
 (Bodega, Existencia,        (Cliente, Pedido,
  MovimientoInventario,       DetallePedido)
  ReservaStock)
```

| Servicio | Puerto | Base de datos | Responsabilidad |
|---|---|---|---|
| Catalogo.Api | 5101 | CatalogoDB_AD01 | Información comercial de los productos |
| Inventario.Api | 5102 | InventarioDB_AD01 | Bodegas, stock, movimientos y reservas |
| Pedidos.Api | 5103 | PedidosDB_AD01 | Clientes y órdenes de compra |

> Las bases llevan el sufijo `_AD01` (el enunciado dice `CatalogoDB`, `InventarioDB`, `PedidosDB`) para no chocar con bases de otras prácticas en el mismo servidor.

## Llaves foráneas: físicas y lógicas
- **FK física** (existe en SQL Server, dentro de la misma base):
  - Existencia, MovimientoInventario y ReservaStock → Bodega (`Restrict`: no se borra una bodega con datos).
  - Pedido → Cliente (`Restrict`: no se borra un cliente con pedidos).
  - DetallePedido → Pedido (`Cascade`: al borrar un pedido se borran sus detalles).
  - Producto → Categoria y Producto → Marca (`Restrict`).
- **FK lógica**: `IdProducto` en Inventario y Pedidos apunta a un producto que vive en **otra base**. SQL Server no puede crear una FK entre bases de microservicios distintos, así que se valida llamando a `Catalogo.Api` por HTTP. Si Catálogo está apagado, la API responde **503** y no se cae.

## Reglas principales
- **Catálogo:** SKU único, precio > 0, estado ACTIVO/INACTIVO; eliminar un producto es una baja lógica (pasa a INACTIVO).
- **Inventario:** el stock solo cambia con movimientos (ENTRADA/SALIDA) y reservas. Stock disponible = Stock − reservas ACTIVAS. Movimientos y confirmación de reservas usan transacción.
- **Pedidos:** cédula ecuatoriana real (provincia, tercer dígito y dígito verificador), correo válido, cédula y correo únicos. El precio se toma de Catálogo y el total lo calcula el servidor. Estados: PENDIENTE → CONFIRMADO → ENTREGADO, y ANULADO desde PENDIENTE o CONFIRMADO.

## Respuestas de error
Todas las APIs responden igual, en español, indicando la acción y el método HTTP:
```json
{
  "mensaje": "ERROR AL REGISTRAR (POST): La cédula está incompleta: debe tener 10 dígitos (tiene 9).",
  "errores": { "cedula": ["La cédula está incompleta: debe tener 10 dígitos (tiene 9)."] }
}
```
Códigos usados: 200, 201, 204, 400, 404 y 503.

## Cómo ejecutar
Requisitos: .NET 10 SDK, SQL Server local con autenticación de Windows y `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

```bash
# 1. Crear las 3 bases (desde la raíz del repositorio)
dotnet ef database update --project src/Catalogo.Api
dotnet ef database update --project src/Inventario.Api
dotnet ef database update --project src/Pedidos.Api

# 2. Cargar datos de prueba (opcional, se puede repetir)
sqlcmd -S localhost -E -C -I -i docs/datos-prueba.sql

# 3. Levantar las 3 APIs (una terminal por cada una)
dotnet run --project src/Catalogo.Api
dotnet run --project src/Inventario.Api
dotnet run --project src/Pedidos.Api
```
Swagger: http://localhost:5101/swagger · http://localhost:5102/swagger · http://localhost:5103/swagger

En Visual Studio: clic derecho en la solución → *Configurar proyectos de inicio* → *Varios proyectos de inicio* → marcar los 3 → F5.

## Cómo probar (Postman)
1. Importar `postman/DISTR-AD01-Microservicios.postman_collection.json` y `postman/Local.postman_environment.json`.
2. Elegir el entorno **Local** y ejecutar la colección con el **Collection Runner**.
3. Resultado esperado: todas las pruebas en verde. Carpetas: 1 Catalogo, 2 Inventario, 3 Pedidos (con el flujo de negocio) y 4 Casos de error.

Guía detallada: [docs/ejecucion.md](docs/ejecucion.md)

## Estructura del repositorio
```
src/            Catalogo.Api, Inventario.Api, Pedidos.Api (Controllers, DTOs, Models, Data, Migrations)
postman/        Colección y entorno de Postman
docs/           Plan, avance, datos de prueba, guía de ejecución, bitácora de IA y órdenes a Codex
```

## Uso de IA
El desarrollo se apoyó en IA (Claude Code como revisor y ChatGPT Codex como implementador). Cada instrucción y su resultado están en `docs/ordenes-codex/` y el resumen en [docs/bitacora-ia.md](docs/bitacora-ia.md).

## Archivos de entrega

- `.gitignore`: excluye `bin/`, `obj/`, `.vs/` y archivos personales de Visual Studio.
- `src/*/appsettings.json`: conexiones a SQL Server y URLs de los servicios.
- `sql/`: creación de las tres bases y esquema completo; ver [instrucciones SQL](sql/README.md).
- `docs/datos-prueba.sql`: datos ficticios de demostración.
- `postman/`: colección JSON y entorno Local listos para importar.
- `docs/Registro_Prompts_y_Fuentes_GuatoJairo.pdf`: registro de uso de IA.

Nombre de entrega: `DISTR-4BM-B1-AD01-DisenioArquitecturasDistribuidas-GuatoJairo`.
