# Avance del proyecto AD01

| Etapa | Estado | Fecha | Notas |
|---|---|---|---|
| 1. Plan | ✅ Lista | 2026-11-05 | docs/plan.md |
| 2. Estructura | ✅ Lista | 2026-11-05 | Orden 01 ejecutada con codex exec y revisada: build 0 errores/0 advertencias, puertos 5101/5102/5103, Swagger 200 |
| 3. Catalogo.Api | ✅ Lista | 2026-11-05 | Órdenes 02, 03 y 04. Base CatalogoDB_AD01. 20/20 pruebas HTTP OK |
| 4. Inventario.Api | ✅ Lista | 2026-11-05 | Órdenes 05 y 06. Base InventarioDB_AD01 (3 FK a Bodega, IdProducto FK lógica). 22/22 pruebas HTTP OK + 503 con Catálogo apagado |
| 5. Pedidos.Api | ✅ Lista | 2026-11-05 | Órdenes 07 y 08. Base PedidosDB_AD01 (Pedido→Cliente Restrict, Detalle→Pedido Cascade, IdProducto FK lógica). 31/31 pruebas HTTP OK + 503 con Catálogo apagado |
| 6. Datos de prueba | ✅ Lista | 2026-11-05 | docs/datos-prueba.sql (repetible, Id fijos) y docs/ejecucion.md. 3 APIs a la vez, Swagger 200 en 5101/5102/5103 |
| 7. Postman | ✅ Lista | 2026-11-05 | Orden 09. postman/ (colección v2.1 + entorno Local). 4 carpetas, 67 peticiones, 188 tests en verde con newman, repetible (2 corridas seguidas) |
| 8. Revisión final | ✅ Lista | 2026-11-05 | Revisión contra rúbrica: 4 hallazgos corregidos (README completo, mensajes de error claros con acción+método en las 3 APIs, cédula ecuatoriana real, justificación sufijo _AD01). Orden 10. Postman 69 peticiones / 209 tests en verde |
| 9. Git y GitHub | ⏳ Pendiente | | |
| 10. Bitácora y sustentación | ⏳ Pendiente | | |
