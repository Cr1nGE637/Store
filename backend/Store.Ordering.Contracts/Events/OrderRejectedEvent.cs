using Store.SharedKernel.Events;

namespace Store.Ordering.Contracts.Events;

[DomainEventName(EventTypeName)]
public record OrderRejectedEvent(
    Guid OrderId,
    Guid? SourceCheckoutId,
    Guid CustomerId,
    string CustomerEmail,
    string Reason,
    IReadOnlyList<OrderItem> Items) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "ordering.order_rejected";
}
