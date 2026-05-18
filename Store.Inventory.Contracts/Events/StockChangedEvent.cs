using Store.SharedKernel.Events;

namespace Store.Inventory.Contracts.Events;

[DomainEventName(EventTypeName)]
public record StockChangedEvent(
    Guid ProductId,
    int AvailableQuantity) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "inventory.stock_changed";
}
