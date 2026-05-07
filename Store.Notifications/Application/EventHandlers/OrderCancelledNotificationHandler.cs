using MediatR;
using Microsoft.Extensions.Logging;
using Store.Notifications.Application.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Notifications.Application.EventHandlers;

public class OrderCancelledNotificationHandler(
    INotificationOutbox outbox,
    INotificationsUnitOfWork unitOfWork,
    ILogger<OrderCancelledNotificationHandler> logger) : INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(notification.CustomerEmail))
        {
            logger.LogWarning("OrderCancelled: no customer email for order {OrderId}, skipping notification", notification.OrderId);
            return;
        }

        try
        {
            var body = $"""
                Your order has been cancelled.

                Order ID: {notification.OrderId}

                If you did not request this cancellation, please contact support.
                """;

            outbox.Enqueue(
                notification.CustomerEmail,
                $"Order #{notification.OrderId} cancelled",
                body);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue notification for order {OrderId}", notification.OrderId);
        }
    }
}
