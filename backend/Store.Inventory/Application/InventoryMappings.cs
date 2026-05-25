using Store.Inventory.Application.DTOs;
using Store.Inventory.Domain.Aggregates;

namespace Store.Inventory.Application;

public static class InventoryMappings
{
    public static StockDto ToDto(StockItem stockItem) =>
        new(stockItem.ProductId, stockItem.Quantity, stockItem.Reserved, stockItem.Available);
}
