using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Carts.Infrastructure.Migrations
{
    public partial class AddProductCacheMainImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainImageUrl",
                schema: "cart",
                table: "ProductCache",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainImageAltText",
                schema: "cart",
                table: "ProductCache",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainImageUrl",
                schema: "cart",
                table: "ProductCache");

            migrationBuilder.DropColumn(
                name: "MainImageAltText",
                schema: "cart",
                table: "ProductCache");
        }
    }
}
