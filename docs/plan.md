# Plan de trabajo – AD01 Microservicios multibodega

## 0. Entorno comprobado (2026-11-05)
| Herramienta | Versión | Comando |
|---|---|---|
| .NET SDK | 10.0.401 (.NET 10, LTS) | `dotnet --list-sdks` |
| SQL Server | 17.0 (SQL Server 2025) Developer, en `localhost`, autenticación de Windows | `sqlcmd -S localhost -E -C -Q "SELECT @@VERSION"` |
| Codex CLI | 0.163.0 | `codex --version` |

Paquetes EF Core: versión 10.x (misma versión mayor que .NET).

## 1. Estructura del repositorio
```
DISTR-[PARALELO]-B1-AD01-DisenioArquitucturasDistribuidas-[ApellidoNombre]/
├── DISTR-...-[ApellidoNombre].sln
├── README.md
├── .gitignore              (dotnet new gitignore → excluye bin/ y obj/)
├── src/
│   ├── Catalogo.Api/       puerto 5101 → CatalogoDB_AD01
│   ├── Inventario.Api/     puerto 5102 → InventarioDB_AD01
│   └── Pedidos.Api/        puerto 5103 → PedidosDB_AD01
├── postman/                colección + entorno v2.1
└── docs/                   plan, avance, órdenes Codex, bitácora IA, PDF declaración IA
```
Cada proyecto: `Controllers/`, `Models/` (entidades), `DTOs/`, `Data/` (DbContext), `Migrations/`.
Inventario y Pedidos además: `Clients/CatalogoClient.cs` (HttpClient tipado).

## 2. Endpoints

Convención de respuestas:
- **200 OK**: consultas, actualizaciones, eliminaciones (con mensaje).
- **201 Created**: al crear (con `CreatedAtAction`, cabecera Location).
- **400 Bad Request**: datos inválidos o regla de negocio violada (mensaje en español).
- **404 Not Found**: el id de la ruta no existe.
- **503 Service Unavailable**: Catálogo no responde (solo Inventario y Pedidos).

### 2.1 Catalogo.Api – http://localhost:5101
| Método | Ruta | Qué hace | Códigos |
|---|---|---|---|
| GET | /api/categorias | Lista categorías | 200 |
| GET | /api/categorias/{id} | Una categoría | 200, 404 |
| POST | /api/categorias | Crea | 201, 400 |
| PUT | /api/categorias/{id} | Actualiza | 200, 400, 404 |
| DELETE | /api/categorias/{id} | Elimina si no tiene productos | 200, 400, 404 |
| GET/GET{id}/POST/PUT/DELETE | /api/marcas | Igual que categorías | igual |
| GET | /api/productos?idCategoria=&idMarca=&estado= | Lista con filtros opcionales; incluye nombre de categoría y marca | 200 |
| GET | /api/productos/{id} | Un producto (**lo usan Inventario y Pedidos**) | 200, 404 |
| POST | /api/productos | Crea (Estado ACTIVO por defecto) | 201, 400 |
| PUT | /api/productos/{id} | Actualiza | 200, 400, 404 |
| DELETE | /api/productos/{id} | **Baja lógica**: pasa a INACTIVO (ver nota) | 200, 404 |

Nota: el producto no se borra físicamente porque Inventario y Pedidos lo referencian por FK lógica; si se borrara quedarían ids "huérfanos" que SQL no puede impedir.

### 2.2 Inventario.Api – http://localhost:5102
| Método | Ruta | Qué hace | Códigos |
|---|---|---|---|
| GET/GET{id}/POST/PUT | /api/bodegas | CRUD de bodegas | 200, 201, 400, 404 |
| DELETE | /api/bodegas/{id} | Solo si no tiene existencias, movimientos ni reservas | 200, 400, 404 |
| GET | /api/existencias?idProducto=&idBodega= | Lista con stock y stock disponible | 200 |
| GET | /api/existencias/{id} | Una existencia | 200, 404 |
| POST | /api/existencias | Crea registro (stock inicial ≥ 0); valida bodega y producto; no repetir (producto, bodega) | 201, 400, 503 |
| GET | /api/movimientos?idProducto=&idBodega=&tipo= | Historial | 200 |
| GET | /api/movimientos/{id} | Un movimiento | 200, 404 |
| POST | /api/movimientos | ENTRADA suma / SALIDA resta stock, en una transacción | 201, 400, 503 |
| GET | /api/reservas?idProducto=&idBodega=&estado= | Lista | 200 |
| GET | /api/reservas/{id} | Una reserva | 200, 404 |
| POST | /api/reservas | Crea reserva ACTIVA si hay stock disponible | 201, 400, 503 |
| PUT | /api/reservas/{id}/confirmar | ACTIVA → CONFIRMADA, descuenta stock y registra SALIDA | 200, 400, 404 |
| PUT | /api/reservas/{id}/cancelar | ACTIVA → CANCELADA | 200, 400, 404 |

No hay PUT/DELETE en existencias ni movimientos: el stock solo cambia por movimientos y reservas (así queda trazabilidad).

### 2.3 Pedidos.Api – http://localhost:5103
| Método | Ruta | Qué hace | Códigos |
|---|---|---|---|
| GET/GET{id}/POST/PUT | /api/clientes | CRUD de clientes | 200, 201, 400, 404 |
| DELETE | /api/clientes/{id} | Solo si no tiene pedidos | 200, 400, 404 |
| GET | /api/pedidos?idCliente=&estado= | Lista | 200 |
| GET | /api/pedidos/{id} | Pedido con sus detalles | 200, 404 |
| POST | /api/pedidos | Body `{ idCliente, detalles:[{ idProducto, cantidad }] }`; precio desde Catálogo; total calculado | 201, 400, 503 |
| PUT | /api/pedidos/{id}/estado | Cambia estado respetando el flujo | 200, 400, 404 |
| DELETE | /api/pedidos/{id} | Solo si está PENDIENTE (borra detalles en cascada) | 200, 400, 404 |

## 3. Reglas de negocio y validaciones
**Catálogo**
- Nombre obligatorio en las 3 tablas; longitudes máximas (ej. Nombre 100, Descripcion 250, SKU 30, PaisOrigen 60).
- SKU obligatorio y **único** (índice único) → repetido: 400.
- Precio decimal(18,2) y **> 0**.
- Estado solo `ACTIVO`/`INACTIVO`, por defecto ACTIVO.
- IdCategoria / IdMarca deben existir → si no: 400.
- No borrar categoría o marca con productos → 400.

**Inventario**
- Stock ≥ 0 (CHECK) y Cantidad > 0 (CHECK).
- Índice único (IdProducto, IdBodega) en Existencia.
- Tipo solo `ENTRADA`/`SALIDA`; Estado reserva `ACTIVA`/`CONFIRMADA`/`CANCELADA` (defecto ACTIVA); Fecha por defecto la actual.
- **Stock disponible = Stock − suma de reservas ACTIVAS** del producto en esa bodega.
- ENTRADA: suma; si no existe la existencia, la crea.
- SALIDA: si cantidad > disponible → 400 "Stock insuficiente" y el stock no cambia.
- Movimiento + actualización de stock dentro de **una transacción** (o se guardan los dos o ninguno).
- Reserva: solo si disponible ≥ cantidad. Confirmar/cancelar solo si está ACTIVA (si no, 400).
- Confirmar: Stock −= cantidad + movimiento SALIDA, en una transacción.

**Pedidos**
- Nombres obligatorio; Cédula 10 dígitos numéricos y **única**; Correo con formato válido y **único** → 400.
- Pedido: cliente existe (400), al menos 1 detalle (400), cantidad > 0 (400), cada producto existe y está ACTIVO en Catálogo (400).
- PrecioUnitario lo toma el servidor de Catálogo; **Total = Σ Cantidad × PrecioUnitario** calculado por el servidor.
- Estado inicial PENDIENTE. Flujo: PENDIENTE → CONFIRMADO → ENTREGADO; ANULADO solo desde PENDIENTE o CONFIRMADO. Otro cambio → 400.
- Total y PrecioUnitario decimal(18,2); Fecha por defecto la actual.

## 4. Llaves foráneas
| Relación | Tipo | Cómo se garantiza |
|---|---|---|
| Producto → Categoria | Física | FK en CatalogoDB_AD01, `Restrict` |
| Producto → Marca | Física | FK en CatalogoDB_AD01, `Restrict` |
| Existencia / MovimientoInventario / ReservaStock → Bodega | Física | FK en InventarioDB_AD01, `Restrict` |
| Pedido → Cliente | Física | FK en PedidosDB_AD01, `Restrict` |
| DetallePedido → Pedido | Física | FK en PedidosDB_AD01, `Cascade` (el detalle no existe sin su pedido) |
| Existencia / Movimiento / Reserva .IdProducto → Producto | **Lógica** | Sin FK en SQL (otra base). Se valida con `GET http://localhost:5101/api/productos/{id}`: 200 = existe, 404 → respondemos 400, sin respuesta → 503 |
| DetallePedido.IdProducto → Producto | **Lógica** | Igual, y además se exige Estado ACTIVO y se lee el Precio |

Física = la base de datos la obliga. Lógica = la obliga el código, porque las tablas están en bases distintas.

## 5. Orden de construcción y cómo probar
| Etapa | Qué se hace | Cómo se prueba |
|---|---|---|
| 2 | Solución, 3 proyectos, puertos, .gitignore | `dotnet build` sin errores; cada API abre Swagger en su puerto |
| 3 | Catalogo.Api: modelo + migración, luego endpoints | CatalogoDB_AD01 en SSMS con 3 tablas y 2 FK; Swagger: 201, 400, 404 |
| 4 | Inventario.Api: modelo + migración, endpoints, reglas de stock, CatalogoClient | Producto inexistente → 400; Catálogo apagado → 503; SALIDA mayor al stock → 400 |
| 5 | Pedidos.Api: modelo + migración, endpoints | Producto INACTIVO → 400; total correcto; ENTREGADO → PENDIENTE → 400 |
| 6 | Datos de prueba inventados y ejecutar las 3 APIs a la vez | Las 3 APIs vivas en 5101/5102/5103 con datos |
| 7 | Colección y entorno Postman v2.1 | Collection Runner todo en verde |
| 8 | Revisión final contra la rúbrica | Tabla requisito / cumple / evidencia / corrección |
| 9 | Git y GitHub | `git status` sin bin/obj; nombre exacto del repo |
| 10 | Bitácora de IA y práctica de sustentación | PDF en docs/ |
