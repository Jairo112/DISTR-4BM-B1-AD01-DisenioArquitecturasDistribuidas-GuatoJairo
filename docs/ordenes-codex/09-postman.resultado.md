Creé la [colección Postman](<C:/Users/ASUS TUF GAMING A16/Desktop/Materias Cuarto/Aplicaciones_Distribuidas/ProductosDeber/DISTR-11PM-B1-AD01-DisenioArquitucturasDistribuidas-GuatoJairo/postman/DISTR-AD01-Microservicios.postman_collection.json>) y el [entorno Local](<C:/Users/ASUS TUF GAMING A16/Desktop/Materias Cuarto/Aplicaciones_Distribuidas/ProductosDeber/DISTR-11PM-B1-AD01-DisenioArquitucturasDistribuidas-GuatoJairo/postman/Local.postman_environment.json>).

Contienen **67 peticiones**, todas con descripción y `pm.test` del código esperado. Ambos JSON y la sintaxis de sus scripts están validados. Eliminé `.gitkeep`; no modifiqué los archivos prohibidos ni ejecuté las APIs.

La carpeta **1 Catalogo** contiene:

- **1.1–1.6 Categorías:** listar **200**, crear **201**, obtener **200**, actualizar **200**, crear temporal **201**, eliminar temporal **200**.
- **1.7–1.12 Marcas:** listar **200**, crear **201**, obtener **200**, actualizar **200**, crear temporal **201**, eliminar temporal **200**.
- **1.13–1.16 Productos:** crear producto del flujo **201**, listar **200**, obtener **200**, actualizar **200**.
- **1.17–1.20 Productos temporales:** crear temporal **201**, eliminarlo **200**, comprobar baja lógica `INACTIVO` **200**, crear producto INACTIVO para errores **201**.

La carpeta **2 Inventario** contiene:

- **2.1–2.6 Bodegas:** listar **200**, crear **201**, obtener **200**, actualizar **200**, crear temporal **201**, eliminar temporal **204**.
- **2.7–2.9 Existencias:** crear con stock cero **201**, listar con filtros **200**, obtener **200**.
- **2.10–2.13 Movimientos:** crear ENTRADA de 20 **201**, listar **200**, obtener **200**, verificar stock 20 **200**.
- **2.14–2.17 Reservas:** crear reserva de 5 **201**, listar **200**, obtener **200**, verificar stock físico 20 y disponible 15 **200**.
- **2.18–2.19 Confirmación:** confirmar reserva **200**, verificar stock físico y disponible 15 **200**.
- **2.20–2.22 Cancelación:** crear segunda reserva **201**, cancelarla **200**, verificar stock 15 y reservado cero **200**.

La carpeta **3 Pedidos** contiene:

- **3.1–3.6 Clientes:** listar **200**, crear **201**, obtener **200**, actualizar **200**, crear temporal **201**, eliminar temporal **204**.
- **3.7–3.9 Pedidos:** crear pedido de 3 unidades, total **75** (**201**), listar con filtros **200**, obtener **200**.
- **3.10–3.11 Pedido temporal:** crear PENDIENTE **201**, eliminarlo **204**.
- **3.12–3.13 Estados:** cambiar a CONFIRMADO **200**, cambiar a ENTREGADO **200**.

La carpeta **4 Casos de error** contiene:

- **4.1–4.3:** id inexistente en cada API → **404**.
- **4.4:** precio negativo → **400**.
- **4.5:** cédula de nueve dígitos → **400**.
- **4.6:** correo inválido → **400**.
- **4.7:** SKU repetido → **400**.
- **4.8:** producto inexistente al crear existencia → **400**.
- **4.9:** SALIDA de 16 con disponible 15 → **400**.
- **4.10:** pedido con producto INACTIVO → **400**.
- **4.11:** transición ENTREGADO → PENDIENTE → **400**.
- **4.12:** eliminar pedido ENTREGADO → **400**.

Las variables se encadenan así: `categoriaId` y `marcaId` permiten crear `productoId`; este y `bodegaId` alimentan `existenciaId`, `movimientoId` y las reservas. Después, `clienteId` y `productoId` permiten crear `pedidoId`, reutilizado para las transiciones y errores. Los temporales tienen ids separados.

Cada iteración completa genera un `sufijo` nuevo y cédulas de diez dígitos cuya disponibilidad se comprueba. Las tres URLs están definidas tanto en la colección como en el entorno.

Con las APIs y migraciones listas, ejecuta desde la raíz:

```powershell
newman run postman/DISTR-AD01-Microservicios.postman_collection.json -e postman/Local.postman_environment.json
```

Ejecuta siempre la colección completa, desde la primera petición y en orden.