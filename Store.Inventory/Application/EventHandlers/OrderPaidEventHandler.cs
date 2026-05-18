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
    IInventoryDomainEventOutbox outbox,
    IInventoryDomainEventInbox inbox,
    ILogger<OrderPaidEventHandler> logger) : INotificationHandler<OrderPaidEvent>
{
    private const string Consumer = nameof(OrderPaidEventHandler);

    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.OrderId, Consumer, cancellationToken))
            return;

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

        foreach (var stockItem in deducted)
            await outbox.AddAsync(stockItem.DomainEvents, cancellationToken);

        inbox.AddProcessed(notification.OrderId, notification.EventType, Consumer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var stockItem in deducted)
            stockItem.ClearDomainEvents();
    }
}
