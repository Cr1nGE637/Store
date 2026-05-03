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
    IPublisher publisher,
    ILogger<OrderCancelledEventHandler> logger) : INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        var released = new List<StockItem>();

        foreach (var item in notification.Items)
        {
            var stockResult = await repository.GetByProductIdAsync(item.ProductId);
            if (stockResult.IsFailure)
            {
                logger.LogWarning("Stock not tracked for product {ProductId}, skipping release", item.ProductId);
                continue;
            }

            var stockItem = stockResult.Value;
            var releaseResult = stockItem.Release(item.Quantity);
            if (releaseResult.IsFailure)
            {
                logger.LogWarning("Cannot release {Quantity} units for product {ProductId}: {Error}",
                    item.Quantity, item.ProductId, releaseResult.Error);
                continue;
            }

            var updateResult = await repository.UpdateAsync(stockItem);
            if (updateResult.IsFailure)
            {
                logger.LogWarning("Failed to update stock for product {ProductId}: {Error}",
                    item.ProductId, updateResult.Error);
                continue;
            }

            released.Add(stockItem);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var stockItem in released)
        {
            foreach (var domainEvent in stockItem.DomainEvents)
                await publisher.Publish(domainEvent, cancellationToken);
            stockItem.ClearDomainEvents();
        }
    }
}
