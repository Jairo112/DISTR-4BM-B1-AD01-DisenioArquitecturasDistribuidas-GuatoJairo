# Bitácora de uso de IA – AD01

| # | Fecha | Herramienta | Para qué | Prompt / orden |
|---|---|---|---|---|
| 1 | 2026-11-05 | Claude Code (Claude Opus 5.5) | Prompt maestro: rol de jefe, reglas del deber y Etapa 1 (plan) | Prompt maestro del estudiante (copiarlo tal cual en la plantilla) |
| 2 | 2026-11-05 | ChatGPT Codex | Etapa 2: crear solución .sln, 3 proyectos Web API, puertos, Swagger, .gitignore y README | docs/ordenes-codex/01-estructura.md |
| 3 | 2026-11-05 | ChatGPT Codex | Etapa 3a: entidades, DbContext y migración de Catalogo.Api | docs/ordenes-codex/02-catalogo-modelo.md |
| 4 | 2026-11-05 | ChatGPT Codex | Etapa 3b: endpoints, DTOs y validaciones de Catalogo.Api | docs/ordenes-codex/03-catalogo-endpoints.md |
| 5 | 2026-11-05 | ChatGPT Codex | Corrección: Range de Precio fallaba por cultura es-EC (500) | docs/ordenes-codex/04-correccion-precio.md |
| 6 | 2026-11-05 | ChatGPT Codex | Etapa 4a (P6): entidades, DbContext y migración de Inventario.Api, FK física vs lógica | docs/ordenes-codex/05-inventario-modelo.md |
| 7 | 2026-11-05 | ChatGPT Codex + revisión Claude Code | Etapa 4b (P7): CatalogoClient, controladores, reglas de stock, reservas y transacciones | docs/ordenes-codex/06-inventario-endpoints.md |
| 8 | 2026-11-05 | ChatGPT Codex | Etapa 5a (P8): entidades, DbContext y migración de Pedidos.Api, Cascade vs Restrict | docs/ordenes-codex/07-pedidos-modelo.md |
| 9 | 2026-11-05 | ChatGPT Codex + revisión Claude Code | Etapa 5b (P9): CatalogoClient, Clientes, Pedidos, total calculado en servidor y flujo de estados | docs/ordenes-codex/08-pedidos-endpoints.md |
| 10 | 2026-11-05 | Claude Code | Etapa 6 (P10): script de datos de prueba y guía para ejecutar las 3 APIs | docs/datos-prueba.sql, docs/ejecucion.md |
| 11 | 2026-11-05 | ChatGPT Codex + verificación Claude Code (newman) | Etapa 7 (P11): colección Postman v2.1 y entorno Local, flujo de negocio y casos de error | docs/ordenes-codex/09-postman.md |
| 12 | 2026-11-05 | Claude Code | Etapa 8 (P12): revisión contra la rúbrica, lista de 4 hallazgos; README completo y cédulas válidas en datos de prueba | README.md, docs/datos-prueba.sql |
| 13 | 2026-11-05 | ChatGPT Codex + verificación Claude Code | Corrección: formato único de errores (acción + método), mensajes en español por campo, validación de cédula ecuatoriana, Postman actualizado | docs/ordenes-codex/10-mensajes-error.md |
