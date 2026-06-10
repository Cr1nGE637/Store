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
                defaultValue: "Accessories");

            migrationBuilder.Sql(
                """
                UPDATE catalog."Categories"
                SET "CategoryCode" = CASE lower(btrim("CategoryName"))
                    WHEN 'processors' THEN 'Processors'
                    WHEN 'motherboards' THEN 'Motherboards'
                    WHEN 'ram' THEN 'RAM'
                    WHEN 'ssd' THEN 'SSD'
                    WHEN 'storage' THEN 'SSD'
                    WHEN 'graphics cards' THEN 'GraphicsCards'
                    WHEN 'power supplies' THEN 'PowerSupplies'
                    WHEN 'laptops' THEN 'Laptops'
                    WHEN 'smartphones' THEN 'Smartphones'
                    WHEN 'accessories' THEN 'Accessories'
                    WHEN 'peripherals' THEN 'Peripherals'
                    WHEN 'monitors' THEN 'Monitors'
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
