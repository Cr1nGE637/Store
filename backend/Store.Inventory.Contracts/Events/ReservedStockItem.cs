namespace Store.Inventory.Contracts.Events;

public record ReservedStockItem(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);
