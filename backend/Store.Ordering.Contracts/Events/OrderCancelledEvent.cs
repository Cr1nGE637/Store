using Store.SharedKernel.Events;

namespace Store.Ordering.Contracts.Events;

[DomainEventName(EventTypeName)]
public record OrderCancelledEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    IReadOnlyList<OrderItem> Items,
    Guid? SourceCheckoutId = null) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "ordering.order_cancelled";
}
