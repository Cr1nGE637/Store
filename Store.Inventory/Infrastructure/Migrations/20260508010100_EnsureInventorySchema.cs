using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Inventory.Infrastructure.DbContexts;

#nullable disable

namespace Store.Inventory.Infrastructure.Migrations
{
    [DbContext(typeof(InventoryDbContext))]
    [Migration("20260508010100_EnsureInventorySchema")]
    public partial class EnsureInventorySchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF to_regclass('public."StockItems"') IS NOT NULL
                       AND to_regclass('inventory."StockItems"') IS NULL THEN
                        ALTER TABLE public."StockItems" SET SCHEMA inventory;
                    END IF;
                END $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
