using Store.SharedKernel.Events;

namespace Store.Catalog.Contracts.Events;

public record ProductCreatedEvent(
    Guid ProductId,
    string ProductName,
    decimal Price,
    Guid CategoryId) : IDomainEvent;
