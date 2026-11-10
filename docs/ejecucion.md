# Cómo ejecutar las 3 APIs

## 1. Crear las bases (una sola vez)
Desde la raíz del repositorio:
```
dotnet ef database update --project src/Catalogo.Api
dotnet ef database update --project src/Inventario.Api
dotnet ef database update --project src/Pedidos.Api
```

## 2. Cargar los datos de prueba
Abrir `docs/datos-prueba.sql` en SSMS y ejecutarlo (F5), o en la terminal:
```
sqlcmd -S localhost -E -C -I -i docs/datos-prueba.sql
```
Se puede repetir: borra todo y vuelve a cargar los datos, con los Id desde 1.

| Base | Datos |
|---|---|
| CatalogoDB_AD01 | 3 categorías, 3 marcas, 6 productos (el 6 está INACTIVO) |
| InventarioDB_AD01 | 2 bodegas (Quito y Guayaquil), 8 existencias con su movimiento de ENTRADA |
| PedidosDB_AD01 | 3 clientes inventados |

## 3. Levantar las 3 APIs a la vez

**Opción A: Visual Studio**
1. Abrir la solución `.sln`.
2. Clic derecho en la solución → *Configurar proyectos de inicio* (*Configure Startup Projects*).
3. Elegir *Varios proyectos de inicio* y poner **Iniciar** en Catalogo.Api, Inventario.Api y Pedidos.Api.
4. Elegir el perfil **http** y presionar F5.

**Opción B: tres terminales** (desde la raíz, una por terminal)
```
dotnet run --project src/Catalogo.Api
dotnet run --project src/Inventario.Api
dotnet run --project src/Pedidos.Api
```

## 4. Comprobar que están vivas (Swagger)
| API | URL |
|---|---|
| Catálogo | http://localhost:5101/swagger |
| Inventario | http://localhost:5102/swagger |
| Pedidos | http://localhost:5103/swagger |

Catálogo debe estar encendido para que Inventario y Pedidos validen productos. Si se apaga, esas dos responden **503**, pero no se caen.

## 5. Probar con Postman

**Importar**
1. Abrir Postman → botón **Import** (arriba a la izquierda).
2. Arrastrar los dos archivos de la carpeta `postman/`:
   - `DISTR-AD01-Microservicios.postman_collection.json` (la colección)
   - `Local.postman_environment.json` (el entorno con las 3 URLs)
3. Arriba a la derecha, en el selector de entornos, elegir **Local**.

**Ejecutar con el Collection Runner**
1. Con las 3 APIs encendidas, clic en los `...` de la colección → **Run collection**.
2. Dejar marcadas todas las peticiones, en orden, con 1 iteración → **Run**.
3. Resultado esperado: 61 peticiones; revisar que todas las comprobaciones de la colección pasen.
   La colección crea sus propios datos con nombres únicos, así que se puede correr varias veces.

**Exportar de nuevo si se modifica**
1. Clic en los `...` de la colección → **Export** → **Collection v2.1** → guardar encima de
   `postman/DISTR-AD01-Microservicios.postman_collection.json`.
2. Para el entorno: **Environments** → `...` de **Local** → **Export** → guardar encima de
   `postman/Local.postman_environment.json`.

**Alternativa por terminal (newman)**
```
npx newman run postman/DISTR-AD01-Microservicios.postman_collection.json -e postman/Local.postman_environment.json
```

