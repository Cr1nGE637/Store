using MediatR;
using Microsoft.Extensions.Logging;
using Store.Notifications.Application.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Notifications.Application.EventHandlers;

public class OrderPaidNotificationHandler(
    INotificationOutbox outbox,
    INotificationsUnitOfWork unitOfWork,
    ILogger<OrderPaidNotificationHandler> logger) : INotificationHandler<OrderPaidEvent>
{
    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(notification.CustomerEmail))
        {
            logger.LogWarning("OrderPaid: no customer email for order {OrderId}, skipping notification", notification.OrderId);
            return;
        }

        try
        {
            var total = notification.Items.Sum(i => i.Price * i.Quantity);
            var body = $"""
                Your payment has been confirmed.

                Order ID: {notification.OrderId}
                Amount paid: {total:C}

                Thank you for your purchase!
                """;

            outbox.Enqueue(
                notification.CustomerEmail,
                $"Payment confirmed for order #{notification.OrderId}",
                body);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue notification for order {OrderId}", notification.OrderId);
        }
    }
}
