using Store.SharedKernel.Events;

namespace Store.Catalog.Contracts.Events;

[DomainEventName(EventTypeName)]
public record ProductMainImageChangedEvent(
    Guid ProductId,
    string ImageUrl,
    string AltText) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "catalog.product_main_image_changed";
}
