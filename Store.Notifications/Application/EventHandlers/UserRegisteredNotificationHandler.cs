using MediatR;
using Microsoft.Extensions.Logging;
using Store.Identity.Contracts.Events;
using Store.Notifications.Application.Interfaces;

namespace Store.Notifications.Application.EventHandlers;

public class UserRegisteredNotificationHandler(
    INotificationOutbox outbox,
    INotificationsUnitOfWork unitOfWork,
    INotificationsDomainEventInbox inbox,
    ILogger<UserRegisteredNotificationHandler> logger) : INotificationHandler<UserRegisteredEvent>
{
    private const string Consumer = nameof(UserRegisteredNotificationHandler);

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        if (string.IsNullOrEmpty(notification.Email))
        {
            logger.LogWarning("UserRegistered: no email for user {UserId}, skipping notification", notification.UserId);
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        try
        {
            var body = $"""
                Welcome to Store, {notification.Name}!

                Your account has been created successfully.
                Email: {notification.Email}

                Start shopping now!
                """;

            await outbox.EnqueueAsync(
                notification.Email,
                "Welcome to Store!",
                body,
                BuildDedupeKey(notification),
                cancellationToken);

            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue welcome notification for user {UserId}", notification.UserId);
            throw;
        }
    }

    private static string BuildDedupeKey(UserRegisteredEvent notification) =>
        $"{notification.EventId}:{Consumer}";
}
