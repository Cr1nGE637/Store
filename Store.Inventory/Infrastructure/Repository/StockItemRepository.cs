using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Inventory.Domain.Aggregates;
using Store.Inventory.Domain.Interfaces;
using Store.Inventory.Infrastructure.DbContexts;
using Store.Inventory.Infrastructure.Entity;

namespace Store.Inventory.Infrastructure.Repository;

public class StockItemRepository(InventoryDbContext context) : IStockItemRepository
{
    public async Task<Result<StockItem>> GetByProductIdAsync(Guid productId)
    {
        var entity = await context.StockItems
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        if (entity == null)
            return Result.Failure<StockItem>($"Stock not found for product {productId}");

        return Result.Success(ToDomain(entity));
    }

    public async Task<Result> AddAsync(StockItem stockItem)
    {
        await context.StockItems.AddAsync(ToEntity(stockItem));
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(StockItem stockItem)
    {
        var entity = await context.StockItems.FindAsync(stockItem.Id);
        if (entity == null)
            return Result.Failure($"StockItem {stockItem.Id} not found for update");

        entity.Quantity = stockItem.Quantity;
        entity.Reserved = stockItem.Reserved;
        return Result.Success();
    }

    private static StockItem ToDomain(StockItemEntity entity) =>
        StockItem.Reconstitute(entity.Id, entity.ProductId, entity.Quantity, entity.Reserved);

    private static StockItemEntity ToEntity(StockItem stockItem) => new()
    {
        Id = stockItem.Id,
        ProductId = stockItem.ProductId,
        Quantity = stockItem.Quantity,
        Reserved = stockItem.Reserved
    };
}
