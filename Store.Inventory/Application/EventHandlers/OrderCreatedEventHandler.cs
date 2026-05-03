using MediatR;
using Microsoft.Extensions.Logging;
using Store.Inventory.Application.Interfaces;
using Store.Inventory.Domain.Aggregates;
using Store.Inventory.Domain.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Inventory.Application.EventHandlers;

public class OrderCreatedEventHandler(
    IStockItemRepository repository,
    IInventoryUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<OrderCreatedEventHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        var reserved = new List<StockItem>();

        foreach (var item in notification.Items)
        {
            var stockResult = await repository.GetByProductIdAsync(item.ProductId);
            if (stockResult.IsFailure)
            {
                logger.LogWarning("Stock not tracked for product {ProductId}, skipping reservation", item.ProductId);
                continue;
            }

            var stockItem = stockResult.Value;
            var reserveResult = stockItem.Reserve(item.Quantity);
            if (reserveResult.IsFailure)
            {
                logger.LogWarning("Cannot reserve {Quantity} units for product {ProductId}: {Error}",
                    item.Quantity, item.ProductId, reserveResult.Error);
                continue;
            }

            var updateResult = await repository.UpdateAsync(stockItem);
            if (updateResult.IsFailure)
            {
                logger.LogWarning("Failed to update stock for product {ProductId}: {Error}",
                    item.ProductId, updateResult.Error);
                continue;
            }

            reserved.Add(stockItem);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var stockItem in reserved)
        {
            foreach (var domainEvent in stockItem.DomainEvents)
                await publisher.Publish(domainEvent, cancellationToken);
            stockItem.ClearDomainEvents();
        }
    }
}
