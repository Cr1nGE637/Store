using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Store.Inventory.Infrastructure.DbContexts;

#nullable disable

namespace Store.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(InventoryDbContext))]
    [Migration("20260522090000_AddStockReservations")]
    public partial class AddStockReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_StockItems_ProductId_NotEmpty",
                schema: "inventory",
                table: "StockItems",
                sql: "\"ProductId\" <> '00000000-0000-0000-0000-000000000000'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockItems_Quantity_NonNegative",
                schema: "inventory",
                table: "StockItems",
                sql: "\"Quantity\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockItems_Reserved_NonNegative",
                schema: "inventory",
                table: "StockItems",
                sql: "\"Reserved\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockItems_Reserved_NotGreaterThanQuantity",
                schema: "inventory",
                table: "StockItems",
                sql: "\"Reserved\" <= \"Quantity\"");

            migrationBuilder.CreateTable(
                name: "StockReservations",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.Id);
                    table.CheckConstraint("CK_StockReservations_OrderId_NotEmpty", "\"OrderId\" <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_StockReservations_Quantity_Positive", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_StockReservations_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalSchema: "inventory",
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_StockItemId_OrderId",
                schema: "inventory",
                table: "StockReservations",
                columns: new[] { "StockItemId", "OrderId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockReservations",
                schema: "inventory");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockItems_ProductId_NotEmpty",
                schema: "inventory",
                table: "StockItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockItems_Quantity_NonNegative",
                schema: "inventory",
                table: "StockItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockItems_Reserved_NonNegative",
                schema: "inventory",
                table: "StockItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockItems_Reserved_NotGreaterThanQuantity",
                schema: "inventory",
                table: "StockItems");
        }
    }
}
