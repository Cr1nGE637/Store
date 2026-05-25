using MediatR;
using Microsoft.Extensions.Logging;
using Store.Notifications.Application.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Notifications.Application.EventHandlers;

public class OrderPaidNotificationHandler(
    INotificationOutbox outbox,
    INotificationsUnitOfWork unitOfWork,
    INotificationsDomainEventInbox inbox,
    ILogger<OrderPaidNotificationHandler> logger) : INotificationHandler<OrderPaidEvent>
{
    private const string Consumer = nameof(OrderPaidNotificationHandler);

    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        if (string.IsNullOrEmpty(notification.CustomerEmail))
        {
            logger.LogWarning("OrderPaid: no customer email for order {OrderId}, skipping notification", notification.OrderId);
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
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

            await outbox.EnqueueAsync(
                notification.CustomerEmail,
                $"Payment confirmed for order #{notification.OrderId}",
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

    private static string BuildDedupeKey(OrderPaidEvent notification) =>
        $"{notification.EventId}:{Consumer}";
}
