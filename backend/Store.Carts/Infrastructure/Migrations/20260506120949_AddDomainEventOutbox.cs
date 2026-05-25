using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Carts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainEventOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "domain_event_outbox",
                schema: "cart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeadLettered = table.Column<bool>(type: "boolean", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domain_event_outbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_domain_event_outbox_ProcessedOnUtc_IsDeadLettered_NextAttem~",
                schema: "cart",
                table: "domain_event_outbox",
                columns: new[] { "ProcessedOnUtc", "IsDeadLettered", "NextAttemptOnUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_event_outbox",
                schema: "cart");
        }
    }
}
