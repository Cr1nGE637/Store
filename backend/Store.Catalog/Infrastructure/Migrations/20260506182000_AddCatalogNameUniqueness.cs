using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Catalog.Infrastructure.DbContexts;

#nullable disable

namespace Store.Catalog.Infrastructure.Migrations
{
    [DbContext(typeof(CatalogDbContext))]
    [Migration("20260506182000_AddCatalogNameUniqueness")]
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
