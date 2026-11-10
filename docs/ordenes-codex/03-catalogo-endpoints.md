# ORDEN 03 – Catalogo.Api: endpoints, DTOs y validaciones

Responde SIEMPRE en español.

## CONTEXTO
Deber universitario AD01: 3 microservicios REST independientes en ASP.NET Core Web API (.NET 10) + SQL Server, cada uno con su base:
- Catalogo.Api (http 5101, base CatalogoDB_AD01) ← ESTA TAREA
- Inventario.Api (5102) y Pedidos.Api (5103): NO se tocan.
Sin gateway, login ni frontend. Arquitectura simple: Controlador + DbContext + DTOs + DataAnnotations. Nombres en español.
Ya existe en `src/Catalogo.Api`: `Models/` (Categoria, Marca, Producto), `Data/CatalogoDbContext.cs` (DbSet Categorias, Marcas, Productos), migración `Inicial` aplicada. Índice único en SKU, CHECK Precio > 0, CHECK Estado IN ('ACTIVO','INACTIVO'), FK Restrict.
Tu entorno NO puede conectarse a SQL Server: no intentes `database update` ni probar contra la base; el jefe hará las pruebas HTTP. Tú solo compila.

## TAREA
Solo en `src/Catalogo.Api`:

### 1. DTOs en `DTOs/`
- `CategoriaCrearDto` (Nombre [Required, StringLength 100], Descripcion [StringLength 250]) y `CategoriaDto` (IdCategoria, Nombre, Descripcion).
- `MarcaCrearDto` (Nombre [Required, StringLength 100], PaisOrigen [StringLength 60]) y `MarcaDto` (IdMarca, Nombre, PaisOrigen).
- `ProductoCrearDto`: IdCategoria [Range(1, int.MaxValue)], IdMarca [Range(1, int.MaxValue)], Nombre [Required, StringLength 150], SKU [Required, StringLength 30], Precio [Range(typeof(decimal), "0.01", "9999999999999999.99")], Estado (string?, opcional) con [RegularExpression("^(ACTIVO|INACTIVO)$")]. Si Estado viene null se guarda "ACTIVO".
- `ProductoDto`: IdProducto, IdCategoria, NombreCategoria, IdMarca, NombreMarca, Nombre, SKU, Precio, Estado.
- Todos los mensajes de error de las DataAnnotations en español (propiedad `ErrorMessage`).
- El mismo DTO de crear se usa para PUT.

### 2. Controladores en `Controllers/` con `[ApiController]` y `[Route("api/[controller]")]` en minúsculas:
- `CategoriasController` → `api/categorias`
- `MarcasController` → `api/marcas`
- `ProductosController` → `api/productos`
(Usa `[Route("api/categorias")]` etc. explícito para que la ruta quede en minúsculas.)

Cada uno con:
| Método | Ruta | Respuesta |
|---|---|---|
| GET | / | 200 con lista de DTO (AsNoTracking) |
| GET | /{id} | 200 DTO o 404 `{ mensaje: "..." }` |
| POST | / | 201 con `CreatedAtAction` y el DTO, o 400 |
| PUT | /{id} | 200 con el DTO actualizado, 404 si no existe, 400 si inválido |
| DELETE | /{id} | 200 `{ mensaje: "..." }`, 404 si no existe |

Reglas específicas:
- **Categorías y Marcas**: DELETE devuelve 400 `"No se puede eliminar la categoría/marca porque tiene productos asociados"` si tiene productos (comprobarlo con `AnyAsync` antes de borrar).
- **Productos**:
  - GET lista acepta filtros opcionales `?idCategoria=&idMarca=&estado=` (estado se compara en mayúsculas).
  - GET devuelve NombreCategoria y NombreMarca (proyección con Select, no Include + mapeo manual pesado).
  - GET /{id} devuelve el producto aunque esté INACTIVO (Inventario y Pedidos lo consultan para validar).
  - POST y PUT: 400 si IdCategoria no existe, 400 si IdMarca no existe, 400 si el SKU ya existe en otro producto (en PUT excluye el propio id). SKU y Estado se guardan con `Trim().ToUpper()`.
  - DELETE es **baja lógica**: cambia Estado a "INACTIVO" y responde 200 `{ mensaje: "Producto dado de baja (INACTIVO)" }`. Comenta en el código por qué: Inventario y Pedidos referencian IdProducto por FK lógica, así que no se borra físicamente.
- Todas las respuestas de error usan el formato `{ mensaje: "texto en español" }`, salvo los 400 automáticos de validación de `[ApiController]` (esos se dejan como están).
- Usa async/await en todo.

### 3. `dotnet build` en la raíz: 0 errores y 0 advertencias.

## REGLAS
- Controladores + DbContext inyectado por constructor + DTOs. Nada más.
- Nunca devuelvas entidades directamente (siempre DTOs).
- Códigos: 200, 201, 400, 404.
- Mensajes al usuario en español.

## CRITERIOS DE ACEPTACIÓN
- [ ] `dotnet build` con 0 errores y 0 advertencias.
- [ ] Swagger muestra los 15 endpoints (5 por controlador).
- [ ] POST producto válido → 201 con cabecera Location.
- [ ] POST producto con Precio 0, sin Nombre o con Estado "OTRO" → 400.
- [ ] POST producto con SKU repetido o IdCategoria inexistente → 400 con mensaje.
- [ ] GET /api/productos/99999 → 404.
- [ ] DELETE categoría con productos → 400.
- [ ] DELETE producto → 200 y el producto queda INACTIVO.

## PROHIBIDO
- Tocar Inventario.Api, Pedidos.Api, docs/, la .sln, `.gitignore`, `appsettings.json`, las migraciones o los modelos.
- Crear migraciones nuevas.
- Repositorios, servicios, interfaces, AutoMapper, MediatR, middleware propio, autenticación, CORS.
- Datos de prueba.
- git, commits o push.

## AL TERMINAR
Explica en español, archivo por archivo, qué hiciste. Explica también el flujo completo de un POST /api/productos, desde que llega la petición hasta que responde 201. Muestra la salida de `dotnet build`.
