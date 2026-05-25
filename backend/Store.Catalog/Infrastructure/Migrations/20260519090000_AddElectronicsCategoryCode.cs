using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Catalog.Infrastructure.DbContexts;

#nullable disable

namespace Store.Catalog.Infrastructure.Migrations
{
    [DbContext(typeof(CatalogDbContext))]
    [Migration("20260519090000_AddElectronicsCategoryCode")]
    public partial class AddElectronicsCategoryCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryCode",
                schema: "catalog",
                table: "Categories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Accessory");

            migrationBuilder.Sql(
                """
                UPDATE catalog."Categories"
                SET "CategoryCode" = CASE lower(btrim("CategoryName"))
                    WHEN 'smartphones' THEN 'Smartphone'
                    WHEN 'laptops' THEN 'Laptop'
                    WHEN 'storage' THEN 'Storage'
                    WHEN 'graphics cards' THEN 'GraphicsCard'
                    WHEN 'monitors' THEN 'Monitor'
                    WHEN 'peripherals' THEN 'Peripheral'
                    WHEN 'accessories' THEN 'Accessory'
                    ELSE "CategoryCode"
                END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryCode",
                schema: "catalog",
                table: "Categories",
                column: "CategoryCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_CategoryCode",
                schema: "catalog",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CategoryCode",
                schema: "catalog",
                table: "Categories");
        }
    }
}
