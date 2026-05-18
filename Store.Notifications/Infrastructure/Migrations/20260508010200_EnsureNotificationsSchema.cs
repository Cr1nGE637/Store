using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.Notifications.Infrastructure.DbContexts;

#nullable disable

namespace Store.Notifications.Infrastructure.Migrations
{
    [DbContext(typeof(NotificationsDbContext))]
    [Migration("20260508010200_EnsureNotificationsSchema")]
    public partial class EnsureNotificationsSchema : Migration
    {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
