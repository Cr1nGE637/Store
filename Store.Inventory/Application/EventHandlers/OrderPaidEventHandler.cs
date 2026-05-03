using MediatR;
using Microsoft.Extensions.Logging;
using Store.Inventory.Application.Interfaces;
using Store.Inventory.Domain.Aggregates;
using Store.Inventory.Domain.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Inventory.Application.EventHandlers;

public class OrderPaidEventHandler(
    IStockItemRepository repository,
    IInventoryUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<OrderPaidEventHandler> logger) : INotificationHandler<OrderPaidEvent>
{
    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        var deducted = new List<StockItem>();

        foreach (var item in notification.Items)
        {
            var stockResult = await repository.GetByProductIdAsync(item.ProductId);
            if (stockResult.IsFailure)
            {
                logger.LogWarning("Stock not tracked for product {ProductId}, skipping deduction", item.ProductId);
                continue;
            }

            var stockItem = stockResult.Value;
            var deductResult = stockItem.Deduct(item.Quantity);
            if (deductResult.IsFailure)
            {
                logger.LogWarning("Cannot deduct {Quantity} units for product {ProductId}: {Error}",
                    item.Quantity, item.ProductId, deductResult.Error);
                continue;
            }

            var updateResult = await repository.UpdateAsync(stockItem);
            if (updateResult.IsFailure)
            {
                logger.LogWarning("Failed to update stock for product {ProductId}: {Error}",
                    item.ProductId, updateResult.Error);
                continue;
            }

            deducted.Add(stockItem);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var stockItem in deducted)
        {
            foreach (var domainEvent in stockItem.DomainEvents)
                await publisher.Publish(domainEvent, cancellationToken);
            stockItem.ClearDomainEvents();
        }
    }
}
