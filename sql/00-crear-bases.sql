-- Crea las bases del deber si todavia no existen. No borra bases ni datos.
USE [master];
GO
IF DB_ID(N'CatalogoDB_AD01') IS NULL CREATE DATABASE [CatalogoDB_AD01];
GO
IF DB_ID(N'InventarioDB_AD01') IS NULL CREATE DATABASE [InventarioDB_AD01];
GO
IF DB_ID(N'PedidosDB_AD01') IS NULL CREATE DATABASE [PedidosDB_AD01];
GO
