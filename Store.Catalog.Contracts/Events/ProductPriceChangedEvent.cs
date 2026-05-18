using Store.SharedKernel.Events;

namespace Store.Catalog.Contracts.Events;

[DomainEventName(EventTypeName)]
public record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "catalog.product_price_changed";
}
