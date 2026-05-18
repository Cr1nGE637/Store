using MediatR;
using Microsoft.Extensions.Logging;
using Store.Inventory.Contracts.Events;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Domain.Interfaces;

namespace Store.Ordering.Application.EventHandlers;

public class OrderStockReservedEventHandler(
    IOrderRepository orderRepository,
    IOrderingUnitOfWork unitOfWork,
    IOrderingDomainEventOutbox outbox,
    IOrderingDomainEventInbox inbox,
    ILogger<OrderStockReservedEventHandler> logger) : INotificationHandler<OrderStockReservedEvent>
{
    private const string Consumer = nameof(OrderStockReservedEventHandler);

    public async Task Handle(OrderStockReservedEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        var orderResult = await orderRepository.GetByIdAsync(notification.OrderId);
        if (orderResult.IsFailure)
            throw new InvalidOperationException(
                $"Order {notification.OrderId} not found while confirming stock reservation");

        var order = orderResult.Value;
        var confirmResult = order.ConfirmStockReserved();
        if (confirmResult.IsFailure)
        {
            logger.LogError(
                "Failed to confirm stock reservation for OrderId={OrderId}: {Error}",
                notification.OrderId,
                confirmResult.Error);
            throw new InvalidOperationException(confirmResult.Error);
        }

        if (confirmResult.Value)
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
