# ORDEN 10 – Mensajes de error claros + validación de cédula ecuatoriana real

Responde SIEMPRE en español.

## CONTEXTO
Deber universitario AD01: 3 microservicios TERMINADOS en `src/` (Catalogo.Api 5101, Inventario.Api 5102, Pedidos.Api 5103) y colección de Postman en `postman/`.
Revisión del docente: los mensajes de error no son claros.
- Inventario y Pedidos responden un mensaje genérico ("Los datos enviados no son válidos...") y no dicen QUÉ campo falló.
- Catálogo responde el formato por defecto de .NET en inglés ("One or more validation errors occurred").
- La cédula solo se valida como 10 dígitos; debe ser una cédula ecuatoriana REAL.
El usuario final no sabe qué es "POST": el mensaje debe decir la acción en palabras y entre paréntesis el método.
Tu entorno NO puede conectarse a SQL Server: no ejecutes las APIs contra la base; el jefe probará. Solo compila y valida JSON.

## FORMATO ÚNICO DE ERROR (las 3 APIs, todos los 400, 404 y 503)
```json
{
  "mensaje": "ERROR AL REGISTRAR (POST): La cédula está incompleta: debe tener 10 dígitos (tiene 9).",
  "errores": { "cedula": ["La cédula está incompleta: debe tener 10 dígitos (tiene 9)."] }
}
```
- Prefijo según el método HTTP de la petición:
  - GET → `ERROR AL CONSULTAR (GET): `
  - POST → `ERROR AL REGISTRAR (POST): `
  - PUT → `ERROR AL ACTUALIZAR (PUT): `
  - DELETE → `ERROR AL ELIMINAR (DELETE): `
- `errores` (diccionario campo → lista de mensajes, nombres de campo en camelCase) SOLO aparece en errores de validación de datos; en errores de negocio (400 de reglas, 404, 503) solo va `mensaje`.
- En validación, `mensaje` = prefijo + los mensajes de todos los campos unidos con un espacio (cada uno termina en punto).
- Las respuestas exitosas (2xx) NO cambian.

## TAREA

### 1. En CADA una de las 3 APIs (código duplicado a propósito: los microservicios son independientes, NO crees un proyecto compartido)
a) `Filtros/MensajeErrorFilter.cs`: un `IAsyncResultFilter` (o `IResultFilter`) registrado globalmente en `AddControllers(o => o.Filters.Add<MensajeErrorFilter>())`. Si el resultado es un `ObjectResult` con código ≥ 400 y su valor tiene la propiedad `mensaje`, lo reemplaza por el formato único anteponiendo el prefijo del método (sin duplicarlo si ya lo tiene). Así NO hay que tocar los `return BadRequest(new { mensaje = "..." })` de los controladores. Comenta en español cómo funciona.
b) `Program.cs`: `InvalidModelStateResponseFactory` que arma `{ mensaje, errores }` desde `ModelState`:
   - Usa el `ErrorMessage` en español de cada DataAnnotation.
   - Si el error viene del lector JSON (tipo incorrecto, JSON mal formado, cuerpo vacío: el error tiene `Exception` o el texto está en inglés / contiene "JSON" / "could not be converted" / "is required" / "body"), reemplázalo por un mensaje claro: `El campo {campo} tiene un formato inválido.` o `No se enviaron datos o el JSON está mal formado.`. Ningún texto en inglés debe llegar al cliente.
   - Quita los prefijos de ruta tipo `$.` o `datos.` de los nombres de campo.
   - El prefijo de método lo pone el filtro (no lo dupliques).
   - En Catalogo.Api esta configuración no existe: agrégala igual.
c) Revisa TODOS los DTOs de entrada: cada `[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]`, `[EmailAddress]` debe tener `ErrorMessage` en español, claro y que diga qué hacer. Ejemplos:
   - "El nombre es obligatorio."
   - "El nombre no puede superar 100 caracteres."
   - "El precio debe ser mayor que 0." (respeta la corrección de cultura del Precio hecha en la orden 04; no la rompas)
   - "La cantidad debe ser mayor que 0."
   - "El tipo debe ser ENTRADA o SALIDA."
   - "El correo no tiene un formato válido (ejemplo: nombre@dominio.com)."
   - "El estado debe ser PENDIENTE, CONFIRMADO, ENTREGADO o ANULADO."
d) Revisa los mensajes de negocio de los controladores: que sean frases completas, claras y en español (ej. "Stock insuficiente. Disponible: 3, solicitado: 5."). No cambies la lógica ni los códigos HTTP.

### 2. Solo en Pedidos.Api: validación de cédula ecuatoriana real
`Validaciones/CedulaEcuatorianaAttribute.cs` (`ValidationAttribute`), reemplaza al `RegularExpression` de la cédula en el DTO de cliente. Devuelve el PRIMER error que encuentre, en este orden:
1. Vacía → "La cédula es obligatoria."
2. Con caracteres no numéricos → "La cédula solo debe contener números (sin guiones ni espacios)."
3. Menos de 10 → "La cédula está incompleta: debe tener 10 dígitos (tiene N)."
4. Más de 10 → "La cédula tiene demasiados dígitos: debe tener 10 (tiene N)."
5. Provincia (2 primeros dígitos) fuera de 01–24 y distinta de 30 → "La cédula no es válida: los dos primeros dígitos (XX) no corresponden a una provincia del Ecuador."
6. Tercer dígito ≥ 6 → "La cédula no es válida: el tercer dígito debe ser menor que 6."
7. Dígito verificador (módulo 10): coeficientes 2,1,2,1,2,1,2,1,2 sobre los 9 primeros dígitos; si un producto es > 9 se le resta 9; se suman; verificador = (10 − suma % 10) % 10; debe ser igual al dígito 10. Si no → "La cédula no es válida: el dígito verificador no coincide. Verifique que sea una cédula real."
Comenta en español el algoritmo paso a paso (lo voy a explicar en la sustentación).

### 3. Postman (`postman/DISTR-AD01-Microservicios.postman_collection.json`)
- El pre-request que genera la cédula ahora debe generar una cédula ecuatoriana VÁLIDA (provincia aleatoria 01–24, tercer dígito 0–5, 6 dígitos aleatorios y el dígito verificador calculado con el algoritmo de arriba).
- En "4 Casos de error":
  - Mantén "Cliente con cédula de nueve dígitos" y agrega un test: `mensaje` empieza con "ERROR AL REGISTRAR (POST):" y contiene "incompleta".
  - Agrega "Cliente con cédula de dígito verificador incorrecto" (usa `1700000002`) → 400, `mensaje` contiene "dígito verificador".
  - Agrega "Producto con datos de tipo incorrecto" (ej. `"precio": "abc"`) → 400 y test de que `mensaje` NO contiene texto en inglés ("JSON", "could not").
  - En el 404 de Catálogo agrega un test: `mensaje` empieza con "ERROR AL CONSULTAR (GET):".
- Renumera para que el orden siga siendo legible. La colección debe seguir corriendo completa y repetible.

### 4. `dotnet build` en la raíz y validar que el JSON de Postman sea válido.

## CRITERIOS DE ACEPTACIÓN
- [ ] `dotnet build` con 0 errores y 0 advertencias.
- [ ] Mismo formato de error en las 3 APIs.
- [ ] Ningún `[Required]`/`[Range]`/etc. sin `ErrorMessage` en español en `src/*/DTOs`.
- [ ] Ningún modelo, migración, código HTTP ni regla de negocio cambiado.

## PROHIBIDO
- Tocar `docs/`, `.gitignore`, la `.sln`, Models, Migrations o DbContext.
- Proyectos compartidos, librerías nuevas (FluentValidation, etc.), middlewares de excepciones globales complejos.
- git init, commits o push.

## AL TERMINAR
Explícame en español, archivo por archivo, qué cambiaste. Explica:
- Cómo funciona el filtro que pone el prefijo (con un ejemplo antes/después).
- El algoritmo de la cédula paso a paso con un ejemplo numérico de una cédula válida.
Muestra la salida de `dotnet build`.
