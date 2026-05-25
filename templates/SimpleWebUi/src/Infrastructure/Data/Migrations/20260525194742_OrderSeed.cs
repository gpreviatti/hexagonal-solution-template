using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class OrderSeed : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var utcNow = DateTime.UtcNow;
        var defaultTimezoneId = "UTC";
        migrationBuilder.InsertData(
            table: "Order",
            columns: ["Id", "Description", "CreatedAt", "CreatedBy", "CreatedByTimezoneId", "DeletedAt", "DeletedBy", "IsDeleted", "Total", "UpdatedAt", "UpdatedBy", "UpdatedByTimezoneId"],
            values: new object[,]
            {
                { 1, "Order 1 description", utcNow, null, defaultTimezoneId, null, null, false, 100.00m, utcNow, null, null },
            });

        migrationBuilder.InsertData(
            table: "Item",
            columns: ["Id", "Name", "CreatedAt", "CreatedBy", "CreatedByTimezoneId", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "OrderId", "Value", "UpdatedAt", "UpdatedBy", "UpdatedByTimezoneId"],
            values: new object[,]
            {
                { 1, "Item 1", utcNow, null, defaultTimezoneId, null, null, "Item 1 description", false, 1, 50.00m, utcNow, null, null },
                { 2, "Item 2", utcNow, null, defaultTimezoneId, null, null, "Item 2 description", false, 1, 50.00m, utcNow, null, null },
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "Item",
            keyColumn: "Id",
            keyValue: 1
        );
        migrationBuilder.DeleteData(
            table: "Item",
            keyColumn: "Id",
            keyValue: 2
        );
        migrationBuilder.DeleteData(
            table: "Order",
            keyColumn: "Id",
            keyValue: 1
        );
    }
}
