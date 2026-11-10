Implementado. Las tres APIs compilan con **0 errores y 0 advertencias**. No ejecuté las APIs ni accedí a SQL Server.

Cambios archivo por archivo:

- `src/Catalogo.Api/Filtros/MensajeErrorFilter.cs`: nuevo filtro global que añade el prefijo según el método HTTP y conserva `errores` cuando corresponde.
- `src/Inventario.Api/Filtros/MensajeErrorFilter.cs`: misma implementación independiente.
- `src/Pedidos.Api/Filtros/MensajeErrorFilter.cs`: misma implementación independiente.
- `src/Catalogo.Api/Program.cs`: registré el filtro y añadí la fábrica de validación con mensajes en español, campos camelCase y detalle por campo.
- `src/Inventario.Api/Program.cs`: reemplacé el mensaje genérico por esa misma estructura.
- `src/Pedidos.Api/Program.cs`: apliqué la misma configuración. Las tres fábricas traducen los errores técnicos y evitan el error adicional del parámetro `datos` cuando ya existe un fallo del lector JSON.
- `src/Pedidos.Api/Validaciones/CedulaEcuatorianaAttribute.cs`: nueva validación de cédula, con los siete controles en el orden solicitado y comentarios explicativos.
- `src/Pedidos.Api/DTOs/ClienteCrearDto.cs`: sustituí la expresión regular por el nuevo atributo y aclaré el mensaje del correo.
- `src/Catalogo.Api/Controllers/CategoriasController.cs`: completé la puntuación del error al eliminar.
- `src/Catalogo.Api/Controllers/MarcasController.cs`: completé la puntuación del mismo error.
- `src/Inventario.Api/Controllers/MovimientosController.cs`: el error de stock muestra disponible y solicitado.
- `src/Inventario.Api/Controllers/ReservasController.cs`: añadí las cantidades disponibles y solicitadas.
- `postman/DISTR-AD01-Microservicios.postman_collection.json`: genera cédulas válidas, conserva esa validez al resolver duplicados, incorpora los casos solicitados y renumera los **14 casos de error**. También corregí la comprobación anterior de `errors` a `errores`.

Revisé las **42 anotaciones** de los DTOs: todas tienen `ErrorMessage` en español. Conservé la configuración de cultura del precio. Los archivos protegidos, códigos HTTP, reglas de negocio y respuestas exitosas permanecen sin cambios.

El filtro actúa justo antes de escribir la respuesta. Si encuentra un `ObjectResult` de error con `mensaje`, añade la acción correspondiente; si ya tiene ese prefijo, lo conserva sin repetirlo.

Antes, en una petición POST:

```json
{"mensaje":"La bodega indicada no existe."}
```

Después:

```json
{"mensaje":"ERROR AL REGISTRAR (POST): La bodega indicada no existe."}
```

En validación también conserva `errores`; en errores de negocio devuelve únicamente `mensaje`.

El algoritmo de cédula primero comprueba vacío, caracteres, longitud, provincia y tercer dígito. Después calcula el verificador. Por ejemplo, **1710034065** cumple las reglas:

1. Tiene diez dígitos numéricos.
2. La provincia es `17`, dentro de 01–24.
3. El tercer dígito es `1`, menor que 6.
4. Los primeros nueve dígitos son `1,7,1,0,0,3,4,0,6`.
5. Multiplicados por `2,1,2,1,2,1,2,1,2` producen `2,7,2,0,0,3,8,0,12`.
6. Al producto `12` se le resta `9`: quedan `2,7,2,0,0,3,8,0,3`, cuya suma es **25**.
7. El verificador es `(10 − 25 % 10) % 10 = 5`, que coincide con el último dígito.

El JSON de Postman es válido y sus **72 scripts** tienen sintaxis válida. Verifiqué además 10 000 cédulas generadas y la resolución de duplicados sin realizar peticiones HTTP.

El `dotnet build` normal encontró un bloqueo de acceso al `NuGet.Config` del perfil. La compilación desde la raíz usando las dependencias ya restauradas terminó correctamente:

```text
> dotnet build --no-restore

Compilación correcta.
    0 Advertencia(s)
    0 Errores

Tiempo transcurrido 00:00:01.15
```