# ORDEN 01 – Estructura de la solución

Responde SIEMPRE en español.

## CONTEXTO
Deber universitario AD01 (Aplicaciones Distribuidas). Son 3 microservicios REST independientes en ASP.NET Core Web API (.NET 10, SDK 10.0.401) con SQL Server, cada uno con su propia base:
- Catalogo.Api (puerto http 5101, base CatalogoDB): Categoria, Marca, Producto.
- Inventario.Api (puerto http 5102, base InventarioDB): Bodega, Existencia, MovimientoInventario, ReservaStock.
- Pedidos.Api (puerto http 5103, base PedidosDB): Cliente, Pedido, DetallePedido.
No hay gateway, login ni frontend: Postman llama directo a cada API. Arquitectura simple: Controlador + DbContext + DTOs + DataAnnotations.
En ESTA tarea solo se crea el esqueleto vacío. Las entidades y la base de datos van en tareas posteriores.

Carpeta raíz del repositorio (ya existe y contiene docs/, NO la borres):
`C:\Users\ASUS TUF GAMING A16\Desktop\Materias Cuarto\Aplicaciones_Distribuidas\ProductosDeber\DISTR-11PM-B1-AD01-DisenioArquitucturasDistribuidas-GuatoJairo`

## TAREA
Trabaja dentro de la carpeta raíz. Usa la CLI de dotnet y muéstrame cada comando que ejecutes.

1. Solución en formato **.sln clásico** (en .NET 10 el valor por defecto es .slnx, NO lo quiero):
   `dotnet new sln -n DISTR-11PM-B1-AD01-DisenioArquitucturasDistribuidas-GuatoJairo --format sln`
2. Tres proyectos Web API con controladores, framework net10.0:
   - `dotnet new webapi -n Catalogo.Api -o src/Catalogo.Api --use-controllers -f net10.0`
   - igual para `Inventario.Api` en `src/Inventario.Api` y `Pedidos.Api` en `src/Pedidos.Api`.
   - Agrégalos a la solución con `dotnet sln add`.
3. En cada proyecto:
   - Borra `Controllers/WeatherForecastController.cs`, `WeatherForecast.cs` y el archivo `<Proyecto>.http`.
   - Instala `Swashbuckle.AspNetCore` (última versión estable compatible con .NET 10) con `dotnet add package`.
   - `Program.cs` debe quedar mínimo:
     ```csharp
     var builder = WebApplication.CreateBuilder(args);
     builder.Services.AddControllers();
     builder.Services.AddEndpointsApiExplorer();
     builder.Services.AddSwaggerGen();
     var app = builder.Build();
     if (app.Environment.IsDevelopment())
     {
         app.UseSwagger();
         app.UseSwaggerUI();
     }
     app.MapControllers();
     app.Run();
     ```
     Quita AddOpenApi/MapOpenApi, UseHttpsRedirection y UseAuthorization (solo usamos http y no hay login). Si quitas AddOpenApi, quita también el paquete Microsoft.AspNetCore.OpenApi.
   - `Properties/launchSettings.json`: deja SOLO el perfil `http` con `applicationUrl` en el puerto que corresponde (Catalogo `http://localhost:5101`, Inventario `http://localhost:5102`, Pedidos `http://localhost:5103`), `launchBrowser: true`, `launchUrl: "swagger"` y `ASPNETCORE_ENVIRONMENT=Development`. Borra el perfil https.
4. En la raíz:
   - `dotnet new gitignore`
   - `postman/.gitkeep` (archivo vacío).
   - `README.md` básico en español: título "AD01 – Diseño de Arquitecturas Distribuidas", autor Jairo Guato, paralelo 11PM, tabla con los 3 servicios, su puerto y su base. Nada más por ahora.
5. Ejecuta `dotnet build` en la raíz.

Estructura final esperada:
```
DISTR-11PM-...-GuatoJairo/
├── DISTR-11PM-B1-AD01-DisenioArquitucturasDistribuidas-GuatoJairo.sln
├── README.md
├── .gitignore
├── docs/          (ya existe, no tocar)
├── postman/.gitkeep
└── src/
    ├── Catalogo.Api/
    ├── Inventario.Api/
    └── Pedidos.Api/
```

## REGLAS
- Los 3 proyectos con net10.0.
- Controladores (NO minimal API). Swagger solo en Development.
- Puertos fijos http: 5101, 5102, 5103.
- Nombres en español como el enunciado.

## CRITERIOS DE ACEPTACIÓN
- [ ] Existe un archivo `.sln` (no `.slnx`) con los 3 proyectos (`dotnet sln list` los muestra).
- [ ] `dotnet build` termina con 0 errores y 0 advertencias.
- [ ] No queda ningún rastro de WeatherForecast (búscalo con grep).
- [ ] Cada `launchSettings.json` tiene un único perfil `http` con su puerto correcto.
- [ ] `.gitignore` contiene reglas que excluyen `bin/` y `obj/` (ej. `[Bb]in/` y `[Oo]bj/`).
- [ ] `dotnet run --project src/Catalogo.Api` abre y `http://localhost:5101/swagger` responde (igual 5102 y 5103). Detén los procesos al terminar.

## PROHIBIDO
- Tocar o borrar la carpeta `docs/`.
- Crear entidades, DbContext, paquetes de Entity Framework, cadenas de conexión o controladores nuevos (eso va en otras órdenes).
- Agregar autenticación, gateway, CORS, Docker, tests, capas extra (repositorios, servicios) o cualquier cosa no pedida.
- Hacer `git init`, commits o push.

## AL TERMINAR
Explícame en español, archivo por archivo, qué creaste o modificaste y por qué. Muéstrame la salida de `dotnet build` y de `dotnet sln list`.
