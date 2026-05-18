using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Notifications.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxRetryAndDlq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notifications");

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF to_regclass('public.outbox_messages') IS NOT NULL
                       AND to_regclass('notifications.outbox_messages') IS NULL THEN
                        ALTER TABLE public.outbox_messages SET SCHEMA notifications;
                    END IF;
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_ProcessedAt",
                schema: "notifications",
                table: "outbox_messages");

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                schema: "notifications",
                table: "outbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeadLettered",
                schema: "notifications",
                table: "outbox_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptAt",
                schema: "notifications",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAt_IsDeadLettered_NextAttemptAt",
                schema: "notifications",
                table: "outbox_messages",
                columns: new[] { "ProcessedAt", "IsDeadLettered", "NextAttemptAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_ProcessedAt_IsDeadLettered_NextAttemptAt",
                schema: "notifications",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                schema: "notifications",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "IsDeadLettered",
                schema: "notifications",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "NextAttemptAt",
                schema: "notifications",
                table: "outbox_messages");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAt",
                schema: "notifications",
                table: "outbox_messages",
                column: "ProcessedAt");
        }
    }
}
