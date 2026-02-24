-- PostgreSQL Seed Data Script

-- Insert Order with explicit ID
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Order" WHERE "Id" = 1 LIMIT 1) THEN
        INSERT INTO "Order" ("Id", "Description", "Total", "CreatedAt", "UpdatedAt")
        VALUES (1, 'XPTO Client Computers', 1000.00, NOW(), NOW());
        
        -- Update sequence to prevent ID conflicts
        SELECT setval(pg_get_serial_sequence('"Order"', 'Id'), (SELECT MAX("Id") FROM "Order"));
    END IF;
END $$;

-- Insert Item with explicit ID
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Item" WHERE "Id" = 1 LIMIT 1) THEN
        INSERT INTO "Item" ("Id", "Name", "Description", "Value", "OrderId", "CreatedAt", "UpdatedAt")
        VALUES (1, 'Graphics Card 4090 Super', 'Nvidia Graphics Cards 24GB RX 4090 Super', 999.00, 1, NOW(), NOW());
        
        -- Update sequence to prevent ID conflicts
        SELECT setval(pg_get_serial_sequence('"Item"', 'Id'), (SELECT MAX("Id") FROM "Item"));
    END IF;
END $$;

-- Insert Notification with explicit ID
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Notification" WHERE "Id" = 1 LIMIT 1) THEN
        INSERT INTO "Notification" ("Id", "NotificationType", "NotificationStatus", "CreatedAt", "UpdatedAt")
        VALUES (1, 'OrderCreated', 'Created', NOW(), NOW());
        
        -- Update sequence to prevent ID conflicts
        SELECT setval(pg_get_serial_sequence('"Notification"', 'Id'), (SELECT MAX("Id") FROM "Notification"));
    END IF;
END $$;
