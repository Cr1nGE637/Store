using Store.SharedKernel.Events;

namespace Store.Ordering.Contracts.Events;

[DomainEventName(EventTypeName)]
public record OrderStockReservationRequestedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    IReadOnlyList<OrderItem> Items) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "ordering.order_stock_reservation_requested";
}
