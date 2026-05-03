using Store.SharedKernel.Events;

namespace Store.Ordering.Contracts.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderItem> Items) : IDomainEvent;
