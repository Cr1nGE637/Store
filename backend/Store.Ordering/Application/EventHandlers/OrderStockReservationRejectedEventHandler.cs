using MediatR;
using Microsoft.Extensions.Logging;
using Store.Inventory.Contracts.Events;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Domain.Interfaces;

namespace Store.Ordering.Application.EventHandlers;

public class OrderStockReservationRejectedEventHandler(
    IOrderRepository orderRepository,
    IOrderingUnitOfWork unitOfWork,
    IOrderingDomainEventOutbox outbox,
    IOrderingDomainEventInbox inbox,
    ILogger<OrderStockReservationRejectedEventHandler> logger) : INotificationHandler<OrderStockReservationRejectedEvent>
{
    private const string Consumer = nameof(OrderStockReservationRejectedEventHandler);

    public async Task Handle(OrderStockReservationRejectedEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        var orderResult = await orderRepository.GetByIdAsync(notification.OrderId);
        if (orderResult.IsFailure)
            throw new InvalidOperationException(
                $"Order {notification.OrderId} not found while rejecting stock reservation");

        var order = orderResult.Value;
        var rejectResult = order.RejectStockReservation(notification.Reason);
        if (rejectResult.IsFailure)
        {
            logger.LogError(
                "Failed to reject stock reservation for OrderId={OrderId}: {Error}",
                notification.OrderId,
                rejectResult.Error);
            throw new InvalidOperationException(rejectResult.Error);
        }

        if (rejectResult.Value)
        {
            var updateResult = await orderRepository.UpdateAsync(order);
            if (updateResult.IsFailure)
                throw new InvalidOperationException(updateResult.Error);

            await outbox.AddAsync(order.DomainEvents, cancellationToken);
        }

        inbox.AddProcessed(notification, Consumer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        order.ClearDomainEvents();
    }
}
