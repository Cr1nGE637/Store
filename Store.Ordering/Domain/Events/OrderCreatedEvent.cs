using Store.SharedKernel.Events;

namespace Store.Ordering.Domain.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderItem> Items) : IDomainEvent;
