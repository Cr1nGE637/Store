using Store.SharedKernel.Events;

namespace Store.Ordering.Contracts.Events;

[DomainEventName(EventTypeName)]
public record OrderCreatedEvent(
    Guid OrderId,
    Guid? SourceCheckoutId,
    Guid CustomerId,
    string CustomerEmail,
    IReadOnlyList<OrderItem> Items) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "ordering.order_created";
}
