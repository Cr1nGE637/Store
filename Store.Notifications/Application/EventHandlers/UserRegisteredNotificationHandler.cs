using MediatR;
using Microsoft.Extensions.Logging;
using Store.Identity.Contracts.Events;
using Store.Notifications.Application.Interfaces;

namespace Store.Notifications.Application.EventHandlers;

public class UserRegisteredNotificationHandler(
    INotificationOutbox outbox,
    INotificationsUnitOfWork unitOfWork,
    ILogger<UserRegisteredNotificationHandler> logger) : INotificationHandler<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(notification.Email))
        {
            logger.LogWarning("UserRegistered: no email for user {UserId}, skipping notification", notification.UserId);
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

            outbox.Enqueue(
                notification.Email,
                "Welcome to Store!",
                body);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue welcome notification for user {UserId}", notification.UserId);
        }
    }
}
