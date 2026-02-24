-- PostgreSQL Migration Script
-- Create database if not exists (handled by Docker environment variable POSTGRES_DB)

-- Create EFMigrationsHistory table if not exists
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Begin transaction
BEGIN;

-- Check if migration already applied
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM "__EFMigrationsHistory"
        WHERE "MigrationId" = '20251012134409_AddNotificationTable'
    ) THEN
        -- Create Notification table
        CREATE TABLE "Notification" (
            "Id" INTEGER GENERATED ALWAYS AS IDENTITY NOT NULL,
            "NotificationType" VARCHAR(100) NOT NULL,
            "NotificationStatus" VARCHAR(100) NOT NULL,
            "Message" VARCHAR(4000) NULL,
            "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
            "CreatedBy" VARCHAR(100) NULL,
            "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
            "UpdatedBy" VARCHAR(100) NULL,
            CONSTRAINT "PK_Notification" PRIMARY KEY ("Id")
        );

        -- Create Order table
        CREATE TABLE "Order" (
            "Id" INTEGER GENERATED ALWAYS AS IDENTITY NOT NULL,
            "Description" VARCHAR(255) NOT NULL,
            "Total" NUMERIC(18,2) NOT NULL,
            "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
            "CreatedBy" VARCHAR(100) NULL,
            "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
            "UpdatedBy" VARCHAR(100) NULL,
            CONSTRAINT "PK_Order" PRIMARY KEY ("Id")
        );

        -- Create Item table
        CREATE TABLE "Item" (
            "Id" INTEGER GENERATED ALWAYS AS IDENTITY NOT NULL,
            "Name" VARCHAR(200) NOT NULL,
            "Description" VARCHAR(255) NOT NULL,
            "Value" NUMERIC(18,2) NOT NULL,
            "OrderId" INTEGER NULL,
            "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
            "CreatedBy" VARCHAR(100) NULL,
            "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
            "UpdatedBy" VARCHAR(100) NULL,
            CONSTRAINT "PK_Item" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_Item_Order_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Order" ("Id")
        );

        -- Create index
        CREATE INDEX "IX_Item_OrderId" ON "Item" ("OrderId");

        -- Insert migration record
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20251012134409_AddNotificationTable', '9.0.8');
    END IF;
END $$;

COMMIT;
