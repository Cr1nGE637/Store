using Microsoft.EntityFrameworkCore;
using Store.Notifications.Application.Interfaces;
using Store.Notifications.Infrastructure.DbContexts;
using Store.Notifications.Infrastructure.Entity;

namespace Store.Notifications.Infrastructure.Services;

public class NotificationOutbox(NotificationsDbContext dbContext) : INotificationOutbox
{
    public async Task EnqueueAsync(string to, string subject, string body, string? dedupeKey, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(dedupeKey) &&
            await dbContext.OutboxMessages.AnyAsync(x => x.DedupeKey == dedupeKey, cancellationToken))
        {
            return;
        }

        dbContext.OutboxMessages.Add(OutboxMessageEntity.Create(to, subject, body, dedupeKey));
    }
}
