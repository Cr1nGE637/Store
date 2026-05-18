using MediatR;
using Microsoft.Extensions.Logging;
using Store.Inventory.Contracts.Events;
using Store.Inventory.Application.Interfaces;
using Store.Inventory.Domain.Aggregates;
using Store.Inventory.Domain.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Inventory.Application.EventHandlers;

public class OrderStockReservationRequestedEventHandler(
    IStockItemRepository repository,
    IInventoryUnitOfWork unitOfWork,
    IInventoryDomainEventOutbox outbox,
    IInventoryDomainEventInbox inbox,
    ILogger<OrderStockReservationRequestedEventHandler> logger) : INotificationHandler<OrderStockReservationRequestedEvent>
{
    private const string Consumer = nameof(OrderStockReservationRequestedEventHandler);

    public async Task Handle(OrderStockReservationRequestedEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        var stockItems = new List<(OrderItem OrderItem, StockItem StockItem)>();

        foreach (var item in notification.Items)
        {
            var stockResult = await repository.GetByProductIdAsync(item.ProductId);
            if (stockResult.IsFailure)
            {
                await RejectAsync(notification, stockResult.Error, cancellationToken);
                return;
            }

            var stockItem = stockResult.Value;
            if (stockItem.Available < item.Quantity)
            {
                var reason = $"Insufficient stock for product {item.ProductId}: available {stockItem.Available}, requested {item.Quantity}";
                await RejectAsync(notification, reason, cancellationToken);
                return;
            }

            stockItems.Add((item, stockItem));
        }

        var reserved = new List<StockItem>();

        foreach (var (item, stockItem) in stockItems)
        {
            var reserveResult = stockItem.Reserve(item.Quantity);
            if (reserveResult.IsFailure)
            {
                logger.LogWarning("Cannot reserve {Quantity} units for product {ProductId} on OrderId={OrderId}: {Error}",
                    item.Quantity, item.ProductId, notification.OrderId, reserveResult.Error);
                throw new InvalidOperationException(reserveResult.Error);
            }

            var updateResult = await repository.UpdateAsync(stockItem);
            if (updateResult.IsFailure)
            {
                logger.LogWarning("Failed to update stock for product {ProductId} on OrderId={OrderId}: {Error}",
                    item.ProductId, notification.OrderId, updateResult.Error);
                throw new InvalidOperationException(updateResult.Error);
            }

            reserved.Add(stockItem);
        }

        foreach (var stockItem in reserved)
            await outbox.AddAsync(stockItem.DomainEvents, cancellationToken);

        await outbox.AddAsync([new OrderStockReservedEvent(
            notification.OrderId,
            notification.CustomerId,
            notification.CustomerEmail,
            notification.Items.Select(ToReservedStockItem).ToList())], cancellationToken);

        inbox.AddProcessed(notification, Consumer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var stockItem in reserved)
            stockItem.ClearDomainEvents();
    }

    private async Task RejectAsync(
        OrderStockReservationRequestedEvent notification,
        string reason,
        CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Rejecting stock reservation for OrderId={OrderId}: {Reason}",
            notification.OrderId,
            reason);

        await outbox.AddAsync([new OrderStockReservationRejectedEvent(
            notification.OrderId,
            notification.CustomerId,
            notification.CustomerEmail,
            reason,
            notification.Items.Select(ToReservedStockItem).ToList())], cancellationToken);

        inbox.AddProcessed(notification, Consumer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ReservedStockItem ToReservedStockItem(OrderItem item) =>
        new(item.ProductId, item.ProductName, item.Price, item.Quantity);
}
