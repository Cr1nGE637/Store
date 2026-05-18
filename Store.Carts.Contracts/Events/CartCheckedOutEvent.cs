using Store.SharedKernel.Events;

namespace Store.Carts.Contracts.Events;

public record CartCheckedOutItem(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);

[DomainEventName(EventTypeName)]
public record CartCheckedOutEvent(
    Guid CartId,
    Guid CustomerId,
    string CustomerEmail,
    IReadOnlyList<CartCheckedOutItem> Items) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "carts.cart_checked_out";
}
