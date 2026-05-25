using Store.SharedKernel.Events;

namespace Store.Catalog.Contracts.Events;

[DomainEventName(EventTypeName)]
public record ProductDeletedEvent(
    Guid ProductId,
    string ProductName) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "catalog.product_deleted";
}
