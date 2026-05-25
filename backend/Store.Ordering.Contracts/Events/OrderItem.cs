namespace Store.Ordering.Contracts.Events;

public record OrderItem(Guid ProductId, string ProductName, decimal Price, int Quantity);
