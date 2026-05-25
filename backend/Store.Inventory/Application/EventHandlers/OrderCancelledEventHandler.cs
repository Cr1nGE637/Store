using MediatR;
using Microsoft.Extensions.Logging;
using Store.Inventory.Application.Interfaces;
using Store.Inventory.Domain.Aggregates;
using Store.Inventory.Domain.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Inventory.Application.EventHandlers;

public class OrderCancelledEventHandler(
    IStockItemRepository repository,
    IInventoryUnitOfWork unitOfWork,
    IInventoryDomainEventOutbox outbox,
    IInventoryDomainEventInbox inbox,
    ILogger<OrderCancelledEventHandler> logger) : INotificationHandler<OrderCancelledEvent>
{
    private const string Consumer = nameof(OrderCancelledEventHandler);

    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        var requestedItems = notification.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new StockMovement(group.Key, group.Sum(item => item.Quantity)))
            .ToList();

        if (requestedItems.Any(item => item.Quantity <= 0))
            throw new InvalidOperationException("Release quantity must be positive");

        var released = new List<StockItem>();

        foreach (var item in requestedItems)
        {
            var stockResult = await repository.GetByProductIdAsync(item.ProductId);
            if (stockResult.IsFailure)
            {
                logger.LogError("Stock not tracked for product {ProductId} while releasing OrderId={OrderId}: {Error}",
                    item.ProductId, notification.OrderId, stockResult.Error);
                throw new InvalidOperationException(stockResult.Error);
            }

            var stockItem = stockResult.Value;
            var releaseResult = stockItem.Release(notification.OrderId, item.Quantity);
            if (releaseResult.IsFailure)
            {
                logger.LogError("Cannot release {Quantity} units for product {ProductId} on OrderId={OrderId}: {Error}",
                    item.Quantity, item.ProductId, notification.OrderId, releaseResult.Error);
                throw new InvalidOperationException(releaseResult.Error);
            }

            var updateResult = await repository.UpdateAsync(stockItem);
            if (updateResult.IsFailure)
            {
                logger.LogError("Failed to update stock for product {ProductId} on OrderId={OrderId}: {Error}",
                    item.ProductId, notification.OrderId, updateResult.Error);
                throw new InvalidOperationException(updateResult.Error);
            }

            released.Add(stockItem);
        }

        foreach (var stockItem in released)
            await outbox.AddAsync(stockItem.DomainEvents, cancellationToken);

        inbox.AddProcessed(notification, Consumer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var stockItem in released)
            stockItem.ClearDomainEvents();
    }

    private sealed record StockMovement(Guid ProductId, int Quantity);
}
