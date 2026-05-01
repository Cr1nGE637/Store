using Store.SharedKernel.Events;

namespace Store.Catalog.Contracts.Events;

public record ProductDeletedEvent(
    Guid ProductId,
    string ProductName) : IDomainEvent;
