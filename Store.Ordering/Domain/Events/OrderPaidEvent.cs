using Store.SharedKernel.Events;

namespace Store.Ordering.Domain.Events;

public record OrderPaidEvent(Guid OrderId, Guid CustomerId) : IDomainEvent;
