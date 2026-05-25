using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Carts.Infrastructure.DbContexts;

#nullable disable

namespace Store.Carts.Infrastructure.Migrations
{
    [DbContext(typeof(CartDbContext))]
    [Migration("20260508020000_AddEventEnvelopeAndInbox")]
    public partial class AddEventEnvelopeAndInbox : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                schema: "cart",
                table: "domain_event_outbox",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "cart",
                table: "domain_event_outbox",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "processed_domain_events",
                schema: "cart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Consumer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_domain_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_domain_event_outbox_EventId",
                schema: "cart",
                table: "domain_event_outbox",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_processed_domain_events_EventId_Consumer",
                schema: "cart",
                table: "processed_domain_events",
                columns: new[] { "EventId", "Consumer" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_domain_events",
                schema: "cart");

            migrationBuilder.DropIndex(
                name: "IX_domain_event_outbox_EventId",
                schema: "cart",
                table: "domain_event_outbox");

            migrationBuilder.DropColumn(
                name: "EventId",
                schema: "cart",
                table: "domain_event_outbox");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "cart",
                table: "domain_event_outbox");
        }
    }
}
