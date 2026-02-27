using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class CreateTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Notification",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                NotificationType = table.Column<string>(type: "text", maxLength: 100, nullable: false),
                NotificationStatus = table.Column<string>(type: "text", maxLength: 100, nullable: false),
                Message = table.Column<string>(type: "text", maxLength: 4000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", maxLength: 100, nullable: true),
                CreatedByTimezoneId = table.Column<string>(type: "text", maxLength: 100, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                UpdatedBy = table.Column<string>(type: "text", maxLength: 100, nullable: true),
                UpdatedByTimezoneId = table.Column<string>(type: "text", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notification", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Order",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Description = table.Column<string>(type: "text", maxLength: 255, nullable: false),
                Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", maxLength: 100, nullable: true),
                CreatedByTimezoneId = table.Column<string>(type: "text", maxLength: 100, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                UpdatedBy = table.Column<string>(type: "text", maxLength: 100, nullable: true),
                UpdatedByTimezoneId = table.Column<string>(type: "text", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Order", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Item",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "text", maxLength: 255, nullable: false),
                Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                OrderId = table.Column<int>(type: "integer", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", maxLength: 100, nullable: true),
                CreatedByTimezoneId = table.Column<string>(type: "text", maxLength: 100, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                UpdatedBy = table.Column<string>(type: "text", maxLength: 100, nullable: true),
                UpdatedByTimezoneId = table.Column<string>(type: "text", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Item", x => x.Id);
                table.ForeignKey(
                    name: "FK_Item_Order_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Order",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Item_OrderId",
            table: "Item",
            column: "OrderId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Item");

        migrationBuilder.DropTable(
            name: "Notification");

        migrationBuilder.DropTable(
            name: "Order");
    }
}
