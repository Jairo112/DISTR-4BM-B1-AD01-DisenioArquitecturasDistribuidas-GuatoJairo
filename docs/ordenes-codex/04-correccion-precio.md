# ORDEN 04 – Corrección: validación de Precio en Catalogo.Api

Responde SIEMPRE en español.

## CONTEXTO
Deber AD01: 3 microservicios ASP.NET Core (.NET 10) + SQL Server. Catalogo.Api (http 5101, base CatalogoDB_AD01) ya tiene modelos, migración, DTOs y controladores (orden 03).
Tu entorno no puede conectarse a SQL Server; el jefe prueba la API.

## PROBLEMA DETECTADO EN LA REVISIÓN
`POST /api/productos` y `PUT /api/productos/{id}` responden **500**:
```
System.ArgumentException: 0.01 is not a valid value for Decimal.
```
Causa: en `src/Catalogo.Api/DTOs/ProductoCrearDto.cs` línea 21,
`[Range(typeof(decimal), "0.01", "9999999999999999.99", ...)]` convierte los límites con la cultura del equipo (es-EC, la coma es el separador decimal) y falla.

## TAREA
En `src/Catalogo.Api/DTOs/ProductoCrearDto.cs`, en ese atributo `Range`, agrega
`ParseLimitsInInvariantCulture = true` y `ConvertValueInInvariantCulture = true`, y cambia el mensaje a
`"El precio debe ser mayor que 0."`. No cambies nada más.
Ejecuta `dotnet build`.

## REGLAS
- Cambio mínimo, solo esa línea.

## CRITERIOS DE ACEPTACIÓN
- [ ] `dotnet build` con 0 errores y 0 advertencias.
- [ ] POST producto con precio 899.99 → 201; con precio 0 → 400 (lo prueba el jefe).

## PROHIBIDO
- Tocar cualquier otro archivo.
- Configurar la cultura global de la aplicación en Program.cs.
- git, commits o push.

## AL TERMINAR
Explica en español qué cambiaste y por qué la cultura del equipo provocaba el error.
