using Store.Inventory.Application.Interfaces;
using Store.Inventory.Contracts;
using Store.Inventory.Domain.Aggregates;
using Store.Inventory.Domain.Interfaces;

namespace Store.Inventory.Application.Services;

public sealed class InventoryStockWriter(
    IStockItemRepository repository,
    IInventoryUnitOfWork unitOfWork,
    IInventoryDomainEventOutbox outbox) : IInventoryStockWriter
{
    public async Task<InventoryStockWriteResult> SetAvailableQuantityAsync(
        Guid productId,
        int availableQuantity,
        CancellationToken cancellationToken)
    {
        if (availableQuantity < 0)
            return InventoryStockWriteResult.Failure("Available quantity cannot be negative");

        var existing = await repository.GetByProductIdAsync(productId);
        StockItem stockItem;

        if (existing.IsFailure)
        {
            var createResult = StockItem.Create(productId, availableQuantity);
            if (createResult.IsFailure)
                return InventoryStockWriteResult.Failure(createResult.Error);

            stockItem = createResult.Value;
            var addResult = await repository.AddAsync(stockItem);
            if (addResult.IsFailure)
                return InventoryStockWriteResult.Failure(addResult.Error);
        }
        else
        {
            stockItem = existing.Value;
            var setResult = stockItem.SetAvailableQuantity(availableQuantity);
            if (setResult.IsFailure)
                return InventoryStockWriteResult.Failure(setResult.Error);

            var updateResult = await repository.UpdateAsync(stockItem);
            if (updateResult.IsFailure)
                return InventoryStockWriteResult.Failure(updateResult.Error);
        }

        await outbox.AddAsync(stockItem.DomainEvents, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        stockItem.ClearDomainEvents();

        return InventoryStockWriteResult.Success();
    }
}
