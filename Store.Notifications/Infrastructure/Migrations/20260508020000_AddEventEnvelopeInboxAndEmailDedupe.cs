using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Notifications.Infrastructure.DbContexts;

#nullable disable

namespace Store.Notifications.Infrastructure.Migrations
{
    [DbContext(typeof(NotificationsDbContext))]
    [Migration("20260508020000_AddEventEnvelopeInboxAndEmailDedupe")]
    public partial class AddEventEnvelopeInboxAndEmailDedupe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DedupeKey",
                schema: "notifications",
                table: "outbox_messages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                schema: "notifications",
                table: "domain_event_outbox",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "notifications",
                table: "domain_event_outbox",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "processed_domain_events",
                schema: "notifications",
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
                name: "IX_outbox_messages_DedupeKey",
                schema: "notifications",
                table: "outbox_messages",
                column: "DedupeKey",
                unique: true,
                filter: "\"DedupeKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_domain_event_outbox_EventId",
                schema: "notifications",
                table: "domain_event_outbox",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_processed_domain_events_EventId_Consumer",
                schema: "notifications",
                table: "processed_domain_events",
                columns: new[] { "EventId", "Consumer" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_domain_events",
                schema: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_DedupeKey",
                schema: "notifications",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_domain_event_outbox_EventId",
                schema: "notifications",
                table: "domain_event_outbox");

            migrationBuilder.DropColumn(
                name: "DedupeKey",
                schema: "notifications",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "EventId",
                schema: "notifications",
                table: "domain_event_outbox");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "notifications",
                table: "domain_event_outbox");
        }
    }
}
