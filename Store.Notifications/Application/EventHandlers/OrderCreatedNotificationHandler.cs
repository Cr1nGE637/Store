using MediatR;
using Microsoft.Extensions.Logging;
using Store.Notifications.Application.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Notifications.Application.EventHandlers;

public class OrderCreatedNotificationHandler(
    INotificationOutbox outbox,
    INotificationsUnitOfWork unitOfWork,
    INotificationsDomainEventInbox inbox,
    ILogger<OrderCreatedNotificationHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    private const string Consumer = nameof(OrderCreatedNotificationHandler);

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        if (string.IsNullOrEmpty(notification.CustomerEmail))
        {
            logger.LogWarning("OrderCreated: no customer email for order {OrderId}, skipping notification", notification.OrderId);
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        try
        {
            var lines = notification.Items.Select(i => $"  - {i.ProductName} x{i.Quantity} - {i.Price:C}");
            var total = notification.Items.Sum(i => i.Price * i.Quantity);
            var body = $"""
                Your order has been placed successfully.

                Order ID: {notification.OrderId}

                Items:
                {string.Join("\n", lines)}

                Total: {total:C}

                Thank you for shopping with us!
                """;

            await outbox.EnqueueAsync(
                notification.CustomerEmail,
                $"Order #{notification.OrderId} placed",
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

    private static string BuildDedupeKey(OrderCreatedEvent notification) =>
        $"{notification.EventId}:{Consumer}";
}
