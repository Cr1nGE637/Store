using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutStockFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_SourceCartId",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "SourceCartId",
                schema: "ordering",
                table: "Orders",
                newName: "SourceCheckoutId");

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                schema: "ordering",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "ordering",
                table: "Orders",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SourceCheckoutId",
                schema: "ordering",
                table: "Orders",
                column: "SourceCheckoutId",
                unique: true,
                filter: "\"SourceCheckoutId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_SourceCheckoutId",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "SourceCheckoutId",
                schema: "ordering",
                table: "Orders",
                newName: "SourceCartId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SourceCartId",
                schema: "ordering",
                table: "Orders",
                column: "SourceCartId",
                unique: true,
                filter: "\"SourceCartId\" IS NOT NULL");
        }
    }
}
