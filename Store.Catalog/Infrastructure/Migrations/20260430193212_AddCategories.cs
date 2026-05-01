using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                schema: "catalog",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "catalog",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "catalog",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                schema: "catalog",
                table: "Products",
                column: "CategoryId",
                principalSchema: "catalog",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "catalog",
                table: "Products");
        }
    }
}
