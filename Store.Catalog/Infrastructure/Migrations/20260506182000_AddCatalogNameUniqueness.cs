using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Catalog.Infrastructure.Migrations
{
    public partial class AddCatalogNameUniqueness : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "UX_Products_ProductName_Normalized"
                ON catalog."Products" (lower(btrim("ProductName")));
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "UX_Categories_CategoryName_Normalized"
                ON catalog."Categories" (lower(btrim("CategoryName")));
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """DROP INDEX IF EXISTS catalog."UX_Categories_CategoryName_Normalized";""");

            migrationBuilder.Sql(
                """DROP INDEX IF EXISTS catalog."UX_Products_ProductName_Normalized";""");
        }
    }
}
