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
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        var requestedItems = notification.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new StockMovement(group.Key, group.Sum(item => item.Quantity)))
            .ToList();

        if (requestedItems.Any(item => item.Quantity <= 0))
            throw new InvalidOperationException("Deduction quantity must be positive");

        var deducted = new List<StockItem>();

        foreach (var item in requestedItems)
        {
            var stockResult = await repository.GetByProductIdAsync(item.ProductId);
            if (stockResult.IsFailure)
            {
                logger.LogError("Stock not tracked for product {ProductId} while deducting OrderId={OrderId}: {Error}",
                    item.ProductId, notification.OrderId, stockResult.Error);
                throw new InvalidOperationException(stockResult.Error);
            }

            var stockItem = stockResult.Value;
            var deductResult = stockItem.Deduct(notification.OrderId, item.Quantity);
            if (deductResult.IsFailure)
            {
                logger.LogError("Cannot deduct {Quantity} units for product {ProductId} on OrderId={OrderId}: {Error}",
                    item.Quantity, item.ProductId, notification.OrderId, deductResult.Error);
                throw new InvalidOperationException(deductResult.Error);
            }

            var updateResult = await repository.UpdateAsync(stockItem);
            if (updateResult.IsFailure)
            {
                logger.LogError("Failed to update stock for product {ProductId} on OrderId={OrderId}: {Error}",
                    item.ProductId, notification.OrderId, updateResult.Error);
                throw new InvalidOperationException(updateResult.Error);
            }

            deducted.Add(stockItem);
        }

        foreach (var stockItem in deducted)
            await outbox.AddAsync(stockItem.DomainEvents, cancellationToken);

        inbox.AddProcessed(notification, Consumer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var stockItem in deducted)
            stockItem.ClearDomainEvents();
    }

    private sealed record StockMovement(Guid ProductId, int Quantity);
}
