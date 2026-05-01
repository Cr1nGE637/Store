using Store.SharedKernel.Events;

namespace Store.Carts.Contracts.Events;

public record CartCheckedOutItem(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);

public record CartCheckedOutEvent(
    Guid CartId,
    Guid CustomerId,
    IReadOnlyList<CartCheckedOutItem> Items) : IDomainEvent;
