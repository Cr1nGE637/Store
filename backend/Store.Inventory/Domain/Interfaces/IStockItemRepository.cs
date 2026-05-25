using CSharpFunctionalExtensions;
using Store.Inventory.Domain.Aggregates;

namespace Store.Inventory.Domain.Interfaces;

public interface IStockItemRepository
{
    Task<Result<StockItem>> GetByProductIdAsync(Guid productId);
    Task<Result> AddAsync(StockItem stockItem);
    Task<Result> UpdateAsync(StockItem stockItem);
}
