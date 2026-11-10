# ORDEN 09 – Colección de Postman (evidencia de pruebas)

Responde SIEMPRE en español.

## CONTEXTO
Deber universitario AD01: 3 microservicios TERMINADOS en `src/`:
- Catalogo.Api (http://localhost:5101): `api/categorias`, `api/marcas`, `api/productos`.
- Inventario.Api (http://localhost:5102): `api/bodegas`, `api/existencias`, `api/movimientos`, `api/reservas` (+ `PUT {id}/confirmar`, `PUT {id}/cancelar`).
- Pedidos.Api (http://localhost:5103): `api/clientes`, `api/pedidos` (+ `PUT {id}/estado`).
LEE los controladores y DTOs de `src/*/Controllers` y `src/*/DTOs` para conocer rutas exactas, cuerpos JSON, reglas y códigos de respuesta reales. NO modifiques nada en `src/`.
Tu entorno NO puede levantar las APIs contra SQL Server: el jefe ejecutará la colección con newman. Tú solo generas los JSON y validas que sean JSON válido.

## TAREA
Crear en `postman/`:

1. `DISTR-AD01-Microservicios.postman_collection.json` (Postman Collection v2.1, schema `https://schema.getpostman.com/json/collection/v2.1.0/collection.json`).
2. `Local.postman_environment.json` con `catalogoUrl = http://localhost:5101`, `inventarioUrl = http://localhost:5102`, `pedidosUrl = http://localhost:5103`. Las URLs de las peticiones usan `{{catalogoUrl}}`, etc. (Agrega también esas 3 variables como variables de colección con los mismos valores, para que funcione aunque no se elija el entorno.)

### Reglas generales
- **La colección debe poder correrse COMPLETA en el Collection Runner, en orden, MUCHAS VECES SEGUIDAS, con todos los tests en verde y SIN depender de los datos de prueba de la base.** Todo lo que use lo crea ella misma.
- Valores únicos: genera en un pre-request de la colección un sufijo único (`Date.now()`), y úsalo en nombres, SKU y correos. La cédula (10 dígitos numéricos, única) se genera con números aleatorios/timestamp en pre-request. Nunca valores fijos que choquen en la segunda corrida.
- Cada petición tiene `pm.test` que verifica el código esperado (`pm.response.to.have.status(...)`). Donde tenga sentido, un segundo test simple sobre el cuerpo (ej. `estado === "CONFIRMADO"`, `total` correcto, `stockDisponible`).
- Guarda los ids creados con `pm.collectionVariables.set(...)` y reúsalos con `{{variable}}` para encadenar.
- Cada petición con `description` breve en español (qué prueba y qué código espera).
- Nombres de peticiones numerados para que se lea el orden (ej. `1.3 Crear producto`).

### Carpetas (en este orden)
**1 Catalogo**: para Categorías, Marcas y Productos: listar, obtener por id, crear, actualizar, eliminar. Para eliminar, crea primero un registro TEMPORAL y elimina ese (no el que usa el flujo). Respeta las reglas de borrado reales de los controladores (ej. no se borra una categoría con productos).
   - Aquí se crea el **producto del flujo** (ACTIVO, precio conocido, ej. 25.00) y un **producto INACTIVO** para los casos de error.
**2 Inventario**: Bodegas (listar, obtener, crear, actualizar, eliminar una temporal); Existencias (listar con filtro, obtener, crear); Movimientos (listar, obtener, crear); Reservas (listar, obtener, crear, confirmar, cancelar). Donde la API no tiene actualizar/eliminar (existencias, movimientos), indícalo en la descripción de la carpeta.
   - Flujo: registrar **ENTRADA** de 20 unidades del producto del flujo en la bodega → crear **reserva** de 5 → **confirmar** reserva (test: estado CONFIRMADA y luego GET de existencias con stock 15). Crea otra reserva y **cancélala**.
**3 Pedidos**: Clientes (listar, obtener, crear, actualizar, eliminar uno temporal sin pedidos); Pedidos (listar con filtro, obtener, crear, eliminar uno PENDIENTE temporal).
   - Flujo: crear **cliente** → crear **pedido** con el producto del flujo (test: total = cantidad × precio, calculado a mano) → cambiar estado PENDIENTE→CONFIRMADO → CONFIRMADO→ENTREGADO.
**4 Casos de error** (cada uno con su pm.test del código):
   - Id inexistente → 404 (al menos uno por API).
   - Datos inválidos → 400 (ej. producto con precio negativo, cliente con cédula de 9 dígitos o correo inválido).
   - SKU repetido → 400.
   - Producto inexistente en inventario (crear existencia o movimiento con IdProducto 999999) → 400.
   - SALIDA con stock insuficiente → 400.
   - Pedido con producto INACTIVO → 400.
   - Cambio de estado inválido (pedido ENTREGADO → PENDIENTE) → 400.
   - Eliminar pedido que no está PENDIENTE → 400.

En la descripción de la colección: en español, qué es, requisitos (3 APIs corriendo, migraciones aplicadas) y el orden del flujo de negocio (crear producto → ENTRADA → reserva → confirmar → cliente → pedido → cambiar estado).

## CRITERIOS DE ACEPTACIÓN
- [ ] Ambos archivos son JSON válido (verifícalo, ej. con `node -e "JSON.parse(require('fs').readFileSync(...))"` o PowerShell `ConvertFrom-Json`).
- [ ] Ningún valor fijo que impida repetir la corrida.
- [ ] Todas las peticiones tienen al menos un `pm.test`.
- [ ] Borra `postman/.gitkeep` (ya no hace falta).

## PROHIBIDO
- Tocar `src/`, `docs/`, `.gitignore`, la `.sln`.
- git init, commits o push.

## AL TERMINAR
Explícame en español qué contiene la colección (lista de peticiones por carpeta con el código esperado) y cómo se encadenan las variables.
