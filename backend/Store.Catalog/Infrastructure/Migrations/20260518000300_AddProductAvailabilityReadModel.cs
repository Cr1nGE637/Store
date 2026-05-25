using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Catalog.Infrastructure.DbContexts;

#nullable disable

namespace Store.Catalog.Infrastructure.Migrations
{
    [DbContext(typeof(CatalogDbContext))]
    [Migration("20260518000300_AddProductAvailabilityReadModel")]
    public partial class AddProductAvailabilityReadModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductAvailabilities",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAvailabilities", x => x.ProductId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAvailabilities_AvailableQuantity",
                schema: "catalog",
                table: "ProductAvailabilities",
                column: "AvailableQuantity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAvailabilities",
                schema: "catalog");
        }
    }
}
