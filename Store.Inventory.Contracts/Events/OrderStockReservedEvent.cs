using Store.SharedKernel.Events;

namespace Store.Inventory.Contracts.Events;

[DomainEventName(EventTypeName)]
public record OrderStockReservedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    IReadOnlyList<ReservedStockItem> Items) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "inventory.order_stock_reserved";
}
