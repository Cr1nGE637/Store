using MediatR;
using Microsoft.Extensions.Logging;
using Store.Notifications.Application.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Notifications.Application.EventHandlers;

public class OrderRejectedNotificationHandler(
    INotificationOutbox outbox,
    INotificationsUnitOfWork unitOfWork,
    INotificationsDomainEventInbox inbox,
    ILogger<OrderRejectedNotificationHandler> logger) : INotificationHandler<OrderRejectedEvent>
{
    private const string Consumer = nameof(OrderRejectedNotificationHandler);

    public async Task Handle(OrderRejectedEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        if (string.IsNullOrEmpty(notification.CustomerEmail))
        {
            logger.LogWarning("OrderRejected: no customer email for order {OrderId}, skipping notification", notification.OrderId);
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        try
        {
            var body = $"""
                Your order could not be completed.

                Order ID: {notification.OrderId}
                Reason: {notification.Reason}

                Reserved stock was not confirmed, so no payment is required.
                """;

            await outbox.EnqueueAsync(
                notification.CustomerEmail,
                $"Order #{notification.OrderId} could not be completed",
                body,
                BuildDedupeKey(notification),
                cancellationToken);

            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue rejection notification for order {OrderId}", notification.OrderId);
            throw;
        }
    }

    private static string BuildDedupeKey(OrderRejectedEvent notification) =>
        $"{notification.EventId}:{Consumer}";
}
