using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Carts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItemProductCacheForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO cart."ProductCache" ("ProductId", "ProductName", "Price", "IsAvailable")
                SELECT DISTINCT ON (item."ProductId")
                    item."ProductId",
                    item."ProductName",
                    item."Price",
                    FALSE
                FROM cart."CartItems" item
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM cart."ProductCache" product
                    WHERE product."ProductId" = item."ProductId"
                )
                ORDER BY item."ProductId", item."CartItemId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                schema: "cart",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductCache_ProductId",
                schema: "cart",
                table: "CartItems",
                column: "ProductId",
                principalSchema: "cart",
                principalTable: "ProductCache",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductCache_ProductId",
                schema: "cart",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ProductId",
                schema: "cart",
                table: "CartItems");
        }
    }
}
