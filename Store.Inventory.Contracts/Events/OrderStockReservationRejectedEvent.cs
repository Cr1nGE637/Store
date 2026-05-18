using Store.SharedKernel.Events;

namespace Store.Inventory.Contracts.Events;

[DomainEventName(EventTypeName)]
public record OrderStockReservationRejectedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    string Reason,
    IReadOnlyList<ReservedStockItem> Items) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "inventory.order_stock_reservation_rejected";
}
