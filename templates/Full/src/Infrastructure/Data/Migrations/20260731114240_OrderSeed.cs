using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class OrderSeed : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Order",
            columns: ["Id", "Description", "Total", "CreatedAt", "CreatedBy", "CreatedByTimezoneId", "UpdatedAt"],
            values: new object[,]
            {
                { 1, "XPTO Client Computers", 1000.00m, DateTimeOffset.UtcNow, "System", "UTC", DateTimeOffset.UtcNow },
                { 2, "Contoso", 1200.00m, DateTimeOffset.UtcNow, "System", "America/New_York", DateTimeOffset.UtcNow },
                { 3, "Order Bob Johnson", 300.00m, DateTimeOffset.UtcNow, "System", "America/New_York", DateTimeOffset.UtcNow },
                { 4, "Order Alice Williams", 300.00m, DateTimeOffset.UtcNow, "System", "America/New_York", DateTimeOffset.UtcNow },
                { 5, "Order John Doe", 300.00m, DateTimeOffset.UtcNow, "System", "America/New_York", DateTimeOffset.UtcNow },
                { 6, "Order David Davis", 300.00m, DateTimeOffset.UtcNow, "System", "America/New_York", DateTimeOffset.UtcNow },
                { 7, "Order Evelyn Wilson", 300.00m, DateTimeOffset.UtcNow, "System", "America/New_York", DateTimeOffset.UtcNow }
            }
        );

        migrationBuilder.InsertData(
            table: "Item",
            columns: ["Id", "Name", "Description", "Value", "CreatedAt", "CreatedBy", "CreatedByTimezoneId", "IsDeleted", "OrderId", "UpdatedAt"],
            values: new object [,]
            {
                { 1, "Graphics Card 4090 Super", "Nvidia Graphics Cards 24GB RX 4090 Super", 1000.00m, DateTimeOffset.UtcNow, "System", "UTC", false, 1, DateTimeOffset.UtcNow },
                { 2, "Notebook", "Notebook with Intel i7 and 16GB RAM", 1200.00m, DateTimeOffset.UtcNow, "System", "America/New_York", false, 1, DateTimeOffset.UtcNow },
                { 3, "Keyboard", "Logitech G213", 100.00m, DateTimeOffset.UtcNow, "System", "America/New_York", false, 2, DateTimeOffset.UtcNow },
                { 4, "Mouse", "Logitech G502", 100.00m, DateTimeOffset.UtcNow, "System", "America/New_York", false, 2, DateTimeOffset.UtcNow },
                { 5, "Monitor", "ASUS VP27UQ", 150.00m, DateTimeOffset.UtcNow, "System", "America/New_York", false, 3, DateTimeOffset.UtcNow },
                { 6, "Headphones", "Sony WH-1000XM4", 150.00m, DateTimeOffset.UtcNow, "System", "America/New_York", false, 3, DateTimeOffset.UtcNow },
                { 7, "Laptop", "Dell XPS 13", 150.00m, DateTimeOffset.UtcNow, "System", "America/New_York", false, 4, DateTimeOffset.UtcNow },
                { 8, "Tablet", "Apple iPad Pro", 150.00m, DateTimeOffset.UtcNow, "System", "America/New_York", false, 4, DateTimeOffset.UtcNow },
                { 9, "Smartphone", "Samsung Galaxy S21", 150.00m, DateTimeOffset.UtcNow, "System", "America/New_York", false, 5, DateTimeOffset.UtcNow }
            }
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "Item",
            keyColumn: "Id",
            keyValues: [1, 2, 3, 4, 5, 6, 7, 8, 9]
        );

        migrationBuilder.DeleteData(
            table: "Order",
            keyColumn: "Id",
            keyValues: [1, 2, 3, 4, 5, 6, 7]
        );
    }
}
