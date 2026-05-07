using Microsoft.EntityFrameworkCore;
using Store.EventOutbox.Infrastructure.Extensions;
using Store.Notifications.Infrastructure.Entity;

namespace Store.Notifications.Infrastructure.DbContexts;

public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessageEntity> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
        modelBuilder.ConfigureDomainEventOutbox();
    }
}
