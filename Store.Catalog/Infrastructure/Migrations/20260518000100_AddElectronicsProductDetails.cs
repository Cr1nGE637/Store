using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Catalog.Infrastructure.Migrations
{
    public partial class AddElectronicsProductDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sku",
                schema: "catalog",
                table: "Products",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                schema: "catalog",
                table: "Products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                schema: "catalog",
                table: "Products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WarrantyMonths",
                schema: "catalog",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE catalog."Products"
                SET "Sku" = concat('LEGACY-', left(replace("ProductId"::text, '-', ''), 16)),
                    "Brand" = 'Generic',
                    "Model" = "ProductName"
                WHERE "Sku" = '';
                """);

            migrationBuilder.CreateTable(
                name: "ProductSpecifications",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSpecifications", x => new { x.ProductId, x.Name });
                    table.ForeignKey(
                        name: "FK_ProductSpecifications_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Brand",
                schema: "catalog",
                table: "Products",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductPrice",
                schema: "catalog",
                table: "Products",
                column: "ProductPrice");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                schema: "catalog",
                table: "Products",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecifications_Name_Value",
                schema: "catalog",
                table: "ProductSpecifications",
                columns: new[] { "Name", "Value" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductSpecifications",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "IX_Products_Brand",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductPrice",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Sku",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Brand",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Model",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Sku",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WarrantyMonths",
                schema: "catalog",
                table: "Products");
        }
    }
}
