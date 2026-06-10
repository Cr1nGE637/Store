namespace Store.Inventory.Contracts;

public interface IInventoryStockWriter
{
    Task<InventoryStockWriteResult> SetAvailableQuantityAsync(
        Guid productId,
        int availableQuantity,
        CancellationToken cancellationToken);
}

public sealed record InventoryStockWriteResult(bool IsSuccess, string Error)
{
    public static InventoryStockWriteResult Success() => new(true, string.Empty);

    public static InventoryStockWriteResult Failure(string error) => new(false, error);
}
