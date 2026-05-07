using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Carts.Infrastructure.Migrations
{
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
