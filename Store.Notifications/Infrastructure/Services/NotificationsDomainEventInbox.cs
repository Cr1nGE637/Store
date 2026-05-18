using Store.EventOutbox.Infrastructure.Services;
using Store.Notifications.Application.Interfaces;
using Store.Notifications.Infrastructure.DbContexts;

namespace Store.Notifications.Infrastructure.Services;

public class NotificationsDomainEventInbox(NotificationsDbContext dbContext)
    : EfDomainEventInbox<NotificationsDbContext>(dbContext), INotificationsDomainEventInbox;
