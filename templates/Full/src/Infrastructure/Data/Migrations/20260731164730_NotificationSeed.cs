using Domain.Notifications;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class NotificationSeed : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.InsertData(
    table: nameof(Notification),
    columns: ["Id", "NotificationType", "NotificationStatus", "CreatedAt", "UpdatedAt", "CreatedBy", "CreatedByTimezoneId"],
    values: new object[,]
    {
        { 1, 1, "Created", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, "System", "UTC" }
    });

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DeleteData(
        table: nameof(Notification),
        keyColumn: "Id",
        keyValues: [1, 2, 3]
    );
}
