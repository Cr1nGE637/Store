namespace Store.Ordering.Domain.Events;

public record OrderItem(Guid ProductId, string ProductName, decimal Price, int Quantity);
