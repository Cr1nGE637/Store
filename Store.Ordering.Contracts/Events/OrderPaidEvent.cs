using Store.SharedKernel.Events;

namespace Store.Ordering.Contracts.Events;

public record OrderPaidEvent(Guid OrderId, Guid CustomerId) : IDomainEvent;
