using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Carts.Infrastructure.DbContexts;

#nullable disable

namespace Store.Carts.Infrastructure.Migrations
{
    [DbContext(typeof(CartDbContext))]
    [Migration("20260508000000_AddCartCheckoutPending")]
    public partial class AddCartCheckoutPending : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCheckoutPending",
                schema: "cart",
                table: "Carts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCheckoutPending",
                schema: "cart",
                table: "Carts");
        }
    }
}
