using Store.Notifications.Application.Interfaces;
using Store.Notifications.Infrastructure.DbContexts;
using Store.Notifications.Infrastructure.Entity;

namespace Store.Notifications.Infrastructure.Services;

public class NotificationOutbox(NotificationsDbContext dbContext) : INotificationOutbox
{
    public void Enqueue(string to, string subject, string body)
    {
        dbContext.OutboxMessages.Add(OutboxMessageEntity.Create(to, subject, body));
    }
}
