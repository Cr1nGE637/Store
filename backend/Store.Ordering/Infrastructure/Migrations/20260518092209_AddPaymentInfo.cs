using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                schema: "ordering",
                table: "Orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTransactionId",
                schema: "ordering",
                table: "Orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAmount",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentTransactionId",
                schema: "ordering",
                table: "Orders");
        }
    }
}
