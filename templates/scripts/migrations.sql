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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240802002835_CreateTables'
)
BEGIN
    CREATE TABLE [Order] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(255) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [CreatedAt] DATETIME2 NULL,
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Order] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240802002835_CreateTables'
)
BEGIN
    CREATE TABLE [Item] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(255) NOT NULL,
        [Value] decimal(18,2) NOT NULL,
        [OrderId] int NULL,
        [CreatedAt] DATETIME2 NULL,
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Item] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Item_Order_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Order] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240802002835_CreateTables'
)
BEGIN
    CREATE INDEX [IX_Item_OrderId] ON [Item] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240802002835_CreateTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240802002835_CreateTables', N'8.0.2');
END;
GO

COMMIT;
GO