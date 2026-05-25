using Store.SharedKernel.Events;

namespace Store.Catalog.Contracts.Events;

[DomainEventName(EventTypeName)]
public record ProductCreatedEvent(
    Guid ProductId,
    string Sku,
    string ProductName,
    string Brand,
    string Model,
    int WarrantyMonths,
    decimal Price,
    Guid CategoryId) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "catalog.product_created";
}
