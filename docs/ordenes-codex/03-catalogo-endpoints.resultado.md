Implementé los seis DTOs y los tres controladores únicamente en `src/Catalogo.Api`. Hay 15 acciones HTTP, con rutas explícitas en minúsculas.

Archivo por archivo:

| Archivo | Cambios |
|---|---|
| `DTOs/CategoriaCrearDto.cs` | Nombre obligatorio, límites de longitud y mensajes en español. Se reutiliza en PUT. |
| `DTOs/CategoriaDto.cs` | Respuesta con identificador, nombre y descripción. |
| `DTOs/MarcaCrearDto.cs` | Nombre obligatorio y límites para nombre y país de origen. Se reutiliza en PUT. |
| `DTOs/MarcaDto.cs` | Respuesta con identificador, nombre y país de origen. |
| `DTOs/ProductoCrearDto.cs` | Validaciones de identificadores, nombre, SKU, precio y estado, con mensajes en español. Se reutiliza en PUT. |
| `DTOs/ProductoDto.cs` | Respuesta del producto con los nombres de categoría y marca. |
| `Controllers/CategoriasController.cs` | CRUD asíncrono, consultas con `AsNoTracking` y bloqueo de eliminación mediante `AnyAsync` si existen productos asociados. |
| `Controllers/MarcasController.cs` | CRUD asíncrono y protección de marcas con productos asociados. |
| `Controllers/ProductosController.cs` | CRUD asíncrono, filtros opcionales, proyección con `Select`, validación de referencias y SKU, normalización y baja lógica comentada. |

El flujo de `POST /api/productos` es:

1. ASP.NET Core convierte el JSON a `ProductoCrearDto`.
2. `[ApiController]` valida las DataAnnotations. Si fallan, responde automáticamente 400.
3. El controlador comprueba que existan la categoría y la marca.
4. Normaliza el SKU con `Trim().ToUpper()` y comprueba que no esté repetido. Estas comprobaciones devuelven 400 con `{ mensaje: "..." }` cuando corresponde.
5. Crea el producto, normaliza Estado y usa `"ACTIVO"` si llega `null`.
6. `SaveChangesAsync()` guarda el registro y obtiene su identificador.
7. Una consulta con `Select` construye el DTO incluyendo los nombres de categoría y marca.
8. `CreatedAtAction` devuelve **201**, el DTO y la cabecera **Location** hacia `/api/productos/{id}`.

La ejecución inicial de `dotnet build` falló porque el entorno deniega el acceso al `NuGet.Config` del perfil de Windows. La compilación desde la raíz usando los paquetes ya restaurados terminó correctamente:

```text
> dotnet build --no-restore

Compilación correcta.
    0 Advertencia(s)
    0 Errores

Tiempo transcurrido 00:00:03.02
```

No ejecuté pruebas HTTP, conexiones a SQL Server ni migraciones. No modifiqué los archivos fuente de Inventario.Api, Pedidos.Api ni los demás archivos prohibidos.