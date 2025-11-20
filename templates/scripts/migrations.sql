IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'OrderDb')
BEGIN
    CREATE DATABASE [OrderDb]
END
GO

USE [OrderDb]
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
    WHERE [MigrationId] = N'20251012134409_AddNotificationTable'
)
BEGIN
    CREATE TABLE [Notification] (
        [Id] int NOT NULL IDENTITY,
        [NotificationType] varchar(100) NOT NULL,
        [NotificationStatus] varchar(100) NOT NULL,
        [Message] varchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] varchar(100) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] varchar(100) NULL,
        CONSTRAINT [PK_Notification] PRIMARY KEY ([Id])
    );

    CREATE TABLE [Order] (
        [Id] int NOT NULL IDENTITY,
        [Description] varchar(255) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] varchar(100) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] varchar(100) NULL,
        CONSTRAINT [PK_Order] PRIMARY KEY ([Id])
    );

    CREATE TABLE [Item] (
        [Id] int NOT NULL IDENTITY,
        [Name] varchar(200) NOT NULL,
        [Description] varchar(255) NOT NULL,
        [Value] decimal(18,2) NOT NULL,
        [OrderId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] varchar(100) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] varchar(100) NULL,
        CONSTRAINT [PK_Item] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Item_Order_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Order] ([Id])
    );

    CREATE INDEX [IX_Item_OrderId] ON [Item] ([OrderId]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251012134409_AddNotificationTable', N'9.0.8');

END;

COMMIT;
GO

