using MediatR;
using Microsoft.Extensions.Logging;
using Store.Notifications.Application.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Notifications.Application.EventHandlers;

public class OrderCancelledNotificationHandler(
    INotificationOutbox outbox,
    INotificationsUnitOfWork unitOfWork,
    INotificationsDomainEventInbox inbox,
    ILogger<OrderCancelledNotificationHandler> logger) : INotificationHandler<OrderCancelledEvent>
{
    private const string Consumer = nameof(OrderCancelledNotificationHandler);

    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        if (string.IsNullOrEmpty(notification.CustomerEmail))
        {
            logger.LogWarning("OrderCancelled: no customer email for order {OrderId}, skipping notification", notification.OrderId);
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        try
        {
            var body = $"""
                Your order has been cancelled.

                Order ID: {notification.OrderId}

                If you did not request this cancellation, please contact support.
                """;

            await outbox.EnqueueAsync(
                notification.CustomerEmail,
                $"Order #{notification.OrderId} cancelled",
                body,
                BuildDedupeKey(notification),
                cancellationToken);

            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue notification for order {OrderId}", notification.OrderId);
            throw;
        }
    }

    private static string BuildDedupeKey(OrderCancelledEvent notification) =>
        $"{notification.EventId}:{Consumer}";
}
