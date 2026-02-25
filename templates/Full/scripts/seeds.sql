DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Order" WHERE "Id" = 1 LIMIT 1) THEN
        INSERT INTO "Order" ("Id", "Description", "Total", "CreatedAt", "UpdatedAt", "CreatedBy", "CreatedByTimezoneId")
        VALUES (1, 'XPTO Client Computers', 2000.00, NOW(), NOW(), 'System', 'UTC'),
               (2, 'Contoso', 1200.00, NOW(), NOW(), 'System', 'America/New_York');
        
        SELECT setval(pg_get_serial_sequence('"Order"', 'Id'), (SELECT MAX("Id") FROM "Order"));
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Item" WHERE "Id" = 1 LIMIT 1) THEN
        INSERT INTO "Item" ("Id", "Name", "Description", "Value", "OrderId", "CreatedAt", "UpdatedAt", "CreatedBy", "CreatedByTimezoneId")
        VALUES (1, 'Graphics Card 4090 Super', 'Nvidia Graphics Cards 24GB RX 4090 Super', 2000.00, 1, NOW(), NOW(), 'System', 'UTC'),
               (3, 'Notebook', 'Notebook with Intel i7 and 16GB RAM', 1200.00, 2, NOW(), NOW(), 'System', 'America/New_York');
        
        SELECT setval(pg_get_serial_sequence('"Item"', 'Id'), (SELECT MAX("Id") FROM "Item"));
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Notification" WHERE "Id" = 1 LIMIT 1) THEN
        INSERT INTO "Notification" ("Id", "NotificationType", "NotificationStatus", "CreatedAt", "UpdatedAt", "CreatedBy", "CreatedByTimezoneId")
        VALUES (1, 'OrderCreated', 'Created', NOW(), NOW(), 'System', 'UTC');
        
        SELECT setval(pg_get_serial_sequence('"Notification"', 'Id'), (SELECT MAX("Id") FROM "Notification"));
    END IF;
END $$;
