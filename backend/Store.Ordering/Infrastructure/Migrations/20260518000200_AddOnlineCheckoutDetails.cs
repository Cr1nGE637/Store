using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Ordering.Infrastructure.DbContexts;

#nullable disable

namespace Store.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(OrderingDbContext))]
    [Migration("20260518000200_AddOnlineCheckoutDetails")]
    public partial class AddOnlineCheckoutDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                schema: "ordering",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "ordering",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                schema: "ordering",
                table: "Orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                schema: "ordering",
                table: "Orders");
        }
    }
}
