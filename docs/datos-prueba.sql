/* =====================================================================
   AD01 - Datos de prueba para las 3 bases (Catálogo, Inventario, Pedidos)
   ---------------------------------------------------------------------
   Requisito: haber aplicado las migraciones de los 3 microservicios.
   Ejecutar completo en SSMS (F5) o con:
     sqlcmd -S localhost -E -C -I -i docs/datos-prueba.sql
   Se puede volver a ejecutar: primero BORRA los datos y reinicia los Id,
   luego inserta todo de nuevo. Los datos son INVENTADOS (no son reales).

   Importante: Inventario y Pedidos guardan IdProducto (FK LÓGICA hacia
   CatalogoDB_AD01). Por eso los productos se insertan con Id fijos
   (1 al 6) usando IDENTITY_INSERT: así los Id coinciden entre las bases.
   ===================================================================== */

/* En cada base: 1) se borran los datos (hijos antes que padres, por las FK),
   2) se reinicia el contador de identidad de las tablas ya usadas
      (DBCC CHECKIDENT ... RESEED 0 => el siguiente Id vuelve a ser 1),
   3) se insertan los datos de prueba. */

/* =========================== 1. CATÁLOGO =========================== */
USE CatalogoDB_AD01;
GO
DELETE FROM Producto;
DELETE FROM Marca;
DELETE FROM Categoria;
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'DBCC CHECKIDENT(''[' + OBJECT_NAME(object_id) + N']'', RESEED, 0) WITH NO_INFOMSGS;'
FROM sys.identity_columns
WHERE last_value IS NOT NULL AND OBJECTPROPERTY(object_id, 'IsUserTable') = 1;
EXEC (@sql);
GO

SET IDENTITY_INSERT Categoria ON;
INSERT INTO Categoria (IdCategoria, Nombre, Descripcion) VALUES
 (1, 'Computadoras', 'Laptops y equipos de escritorio'),
 (2, 'Periféricos',  'Teclados, mouse y monitores'),
 (3, 'Almacenamiento', 'Discos y memorias');
SET IDENTITY_INSERT Categoria OFF;

SET IDENTITY_INSERT Marca ON;
INSERT INTO Marca (IdMarca, Nombre, PaisOrigen) VALUES
 (1, 'Lenovo',   'China'),
 (2, 'Logitech', 'Suiza'),
 (3, 'Kingston', 'Estados Unidos');
SET IDENTITY_INSERT Marca OFF;

SET IDENTITY_INSERT Producto ON;
INSERT INTO Producto (IdProducto, IdCategoria, IdMarca, Nombre, SKU, Precio, Estado) VALUES
 (1, 1, 1, 'Laptop IdeaPad 15',        'LEN-IDP15', 650.00, 'ACTIVO'),
 (2, 1, 1, 'Mini PC ThinkCentre',      'LEN-TCM1',  480.00, 'ACTIVO'),
 (3, 2, 2, 'Mouse inalámbrico M185',   'LOG-M185',   15.50, 'ACTIVO'),
 (4, 2, 2, 'Teclado K120',             'LOG-K120',   12.75, 'ACTIVO'),
 (5, 3, 3, 'Memoria USB 64 GB',        'KIN-USB64',   9.99, 'ACTIVO'),
 (6, 3, 3, 'Disco SSD 240 GB (desc.)', 'KIN-SSD240', 35.00, 'INACTIVO');  -- producto INACTIVO para probar el 400 en Pedidos
SET IDENTITY_INSERT Producto OFF;
GO

/* ========================== 2. INVENTARIO ========================== */
USE InventarioDB_AD01;
GO
DELETE FROM ReservaStock;
DELETE FROM MovimientoInventario;
DELETE FROM Existencia;
DELETE FROM Bodega;
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'DBCC CHECKIDENT(''[' + OBJECT_NAME(object_id) + N']'', RESEED, 0) WITH NO_INFOMSGS;'
FROM sys.identity_columns
WHERE last_value IS NOT NULL AND OBJECTPROPERTY(object_id, 'IsUserTable') = 1;
EXEC (@sql);
GO

SET IDENTITY_INSERT Bodega ON;
INSERT INTO Bodega (IdBodega, Nombre, Ciudad, Direccion) VALUES
 (1, 'Bodega Norte', 'Quito',     'Av. de la Prensa N45-12'),
 (2, 'Bodega Sur',   'Guayaquil', 'Av. 25 de Julio, km 2');
SET IDENTITY_INSERT Bodega OFF;

-- Existencias iniciales (IdProducto = FK lógica hacia Catálogo).
-- El producto 6 está INACTIVO y no tiene stock.
INSERT INTO Existencia (IdProducto, IdBodega, Stock) VALUES
 (1, 1, 10), (1, 2, 5),
 (2, 1,  4),
 (3, 1, 50), (3, 2, 30),
 (4, 1, 40), (4, 2, 25),
 (5, 2, 100);

-- Cada stock inicial queda respaldado por un movimiento de ENTRADA.
INSERT INTO MovimientoInventario (IdProducto, IdBodega, Tipo, Cantidad, Fecha)
SELECT IdProducto, IdBodega, 'ENTRADA', Stock, GETDATE()
FROM Existencia;
GO

/* =========================== 3. PEDIDOS ============================ */
USE PedidosDB_AD01;
GO
DELETE FROM DetallePedido;
DELETE FROM Pedido;
DELETE FROM Cliente;
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'DBCC CHECKIDENT(''[' + OBJECT_NAME(object_id) + N']'', RESEED, 0) WITH NO_INFOMSGS;'
FROM sys.identity_columns
WHERE last_value IS NOT NULL AND OBJECTPROPERTY(object_id, 'IsUserTable') = 1;
EXEC (@sql);
GO

-- Clientes inventados. Las cédulas son ficticias pero cumplen el algoritmo
-- de la cédula ecuatoriana (provincia, tercer dígito y dígito verificador).
SET IDENTITY_INSERT Cliente ON;
INSERT INTO Cliente (IdCliente, Nombres, Cedula, Correo) VALUES
 (1, 'María Fernanda Pérez', '1700000001', 'maria.perez@correo-prueba.ec'),
 (2, 'Carlos Andrés Mora',   '0900000001', 'carlos.mora@correo-prueba.ec'),
 (3, 'Lucía Torres Vega',    '0100000009', 'lucia.torres@correo-prueba.ec');
SET IDENTITY_INSERT Cliente OFF;
GO

/* Resumen: cuántos registros quedaron en cada tabla. */
SELECT 'Catalogo'   AS Base, 'Categoria' AS Tabla, COUNT(*) AS Filas FROM CatalogoDB_AD01.dbo.Categoria
UNION ALL SELECT 'Catalogo',   'Marca',                COUNT(*) FROM CatalogoDB_AD01.dbo.Marca
UNION ALL SELECT 'Catalogo',   'Producto',             COUNT(*) FROM CatalogoDB_AD01.dbo.Producto
UNION ALL SELECT 'Inventario', 'Bodega',               COUNT(*) FROM InventarioDB_AD01.dbo.Bodega
UNION ALL SELECT 'Inventario', 'Existencia',           COUNT(*) FROM InventarioDB_AD01.dbo.Existencia
UNION ALL SELECT 'Inventario', 'MovimientoInventario', COUNT(*) FROM InventarioDB_AD01.dbo.MovimientoInventario
UNION ALL SELECT 'Pedidos',    'Cliente',              COUNT(*) FROM PedidosDB_AD01.dbo.Cliente;
GO
