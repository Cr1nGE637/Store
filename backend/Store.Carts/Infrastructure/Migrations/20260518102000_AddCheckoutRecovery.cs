using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Carts.Infrastructure.DbContexts;

#nullable disable

namespace Store.Carts.Infrastructure.Migrations
{
    [DbContext(typeof(CartDbContext))]
    [Migration("20260518102000_AddCheckoutRecovery")]
    public partial class AddCheckoutRecovery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CheckoutPendingSince",
                schema: "cart",
                table: "Carts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PendingCheckoutId",
                schema: "cart",
                table: "Carts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CheckoutOrders",
                schema: "cart",
                columns: table => new
                {
                    CheckoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoutOrders", x => x.CheckoutId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CheckoutPendingSince",
                schema: "cart",
                table: "Carts",
                column: "CheckoutPendingSince");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutOrders_CustomerId",
                schema: "cart",
                table: "CheckoutOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutOrders_OrderId",
                schema: "cart",
                table: "CheckoutOrders",
                column: "OrderId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckoutOrders",
                schema: "cart");

            migrationBuilder.DropIndex(
                name: "IX_Carts_CheckoutPendingSince",
                schema: "cart",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "CheckoutPendingSince",
                schema: "cart",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "PendingCheckoutId",
                schema: "cart",
                table: "Carts");
        }
    }
}
