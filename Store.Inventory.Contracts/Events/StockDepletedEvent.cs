using Store.SharedKernel.Events;

namespace Store.Inventory.Contracts.Events;

[DomainEventName(EventTypeName)]
public record StockDepletedEvent(Guid ProductId) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "inventory.stock_depleted";
}
