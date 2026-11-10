USE [CatalogoDB_AD01];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105163934_Inicial'
)
BEGIN
    CREATE TABLE [Categoria] (
        [IdCategoria] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(250) NULL,
        CONSTRAINT [PK_Categoria] PRIMARY KEY ([IdCategoria])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105163934_Inicial'
)
BEGIN
    CREATE TABLE [Marca] (
        [IdMarca] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [PaisOrigen] nvarchar(60) NULL,
        CONSTRAINT [PK_Marca] PRIMARY KEY ([IdMarca])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105163934_Inicial'
)
BEGIN
    CREATE TABLE [Producto] (
        [IdProducto] int NOT NULL IDENTITY,
        [IdCategoria] int NOT NULL,
        [IdMarca] int NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [SKU] nvarchar(30) NOT NULL,
        [Precio] decimal(18,2) NOT NULL,
        [Estado] nvarchar(10) NOT NULL DEFAULT ('ACTIVO'),
        CONSTRAINT [PK_Producto] PRIMARY KEY ([IdProducto]),
        CONSTRAINT [CK_Producto_Estado] CHECK ([Estado] IN ('ACTIVO','INACTIVO')),
        CONSTRAINT [CK_Producto_Precio] CHECK ([Precio] > 0),
        CONSTRAINT [FK_Producto_Categoria_IdCategoria] FOREIGN KEY ([IdCategoria]) REFERENCES [Categoria] ([IdCategoria]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Producto_Marca_IdMarca] FOREIGN KEY ([IdMarca]) REFERENCES [Marca] ([IdMarca]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105163934_Inicial'
)
BEGIN
    CREATE INDEX [IX_Producto_IdCategoria] ON [Producto] ([IdCategoria]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105163934_Inicial'
)
BEGIN
    CREATE INDEX [IX_Producto_IdMarca] ON [Producto] ([IdMarca]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105163934_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Producto_SKU] ON [Producto] ([SKU]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105163934_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105163934_Inicial', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107175237_AgregarEstadoCategoria'
)
BEGIN
    ALTER TABLE [Categoria] ADD [Estado] nvarchar(10) NOT NULL DEFAULT N'ACTIVO';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107175237_AgregarEstadoCategoria'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261107175237_AgregarEstadoCategoria', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107222445_AgregarEstadoMarca'
)
BEGIN
    ALTER TABLE [Marca] ADD [Estado] nvarchar(10) NOT NULL DEFAULT N'ACTIVO';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107222445_AgregarEstadoMarca'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261107222445_AgregarEstadoMarca', N'10.0.14');
END;

COMMIT;
GO

