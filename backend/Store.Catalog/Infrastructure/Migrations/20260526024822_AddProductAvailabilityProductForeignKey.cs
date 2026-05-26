using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAvailabilityProductForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM catalog."ProductAvailabilities" availability
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM catalog."Products" product
                    WHERE product."ProductId" = availability."ProductId"
                );
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAvailabilities_Products_ProductId",
                schema: "catalog",
                table: "ProductAvailabilities",
                column: "ProductId",
                principalSchema: "catalog",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAvailabilities_Products_ProductId",
                schema: "catalog",
                table: "ProductAvailabilities");
        }
    }
}
