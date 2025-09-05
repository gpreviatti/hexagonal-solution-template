USE [OrderDb]
GO

SET IDENTITY_INSERT [Order] ON
GO

IF NOT EXISTS (SELECT TOP 1 * FROM [Order] WHERE [Id] = 1) BEGIN
    INSERT INTO [Order] ([Id], [Description], [Total], [CreatedAt], [UpdatedAt])
    VALUES (1, 'XPTO Client Computers', 1000.00, GETDATE(), GETDATE());
END
GO

SET IDENTITY_INSERT [Order] OFF
GO

SET IDENTITY_INSERT [Item] ON
GO

IF NOT EXISTS (SELECT TOP 1 * FROM [Item] WHERE [Id] = 1 ) BEGIN
    INSERT INTO [Item] ([Id], [Name], [Description], [Value], [OrderId], [CreatedAt], [UpdatedAt])
    VALUES (1, 'Graphics Card 4090 Super', 'Nvidia Graphics Cards 24GB RX 4090 Super', 999.00, 1, GETDATE(), GETDATE());
END
GO

SET IDENTITY_INSERT [Item] OFF
GO