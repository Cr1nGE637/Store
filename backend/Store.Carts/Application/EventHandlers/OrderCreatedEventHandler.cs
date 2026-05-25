using MediatR;
using Microsoft.Extensions.Logging;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Carts.Application.EventHandlers;

public class OrderCreatedEventHandler(
    ICartRepository cartRepository,
    ICartUnitOfWork unitOfWork,
    ICartDomainEventInbox inbox,
    ICheckoutOrderRecorder checkoutOrderRecorder,
    ILogger<OrderCreatedEventHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    private const string Consumer = nameof(OrderCreatedEventHandler);

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        if (notification.SourceCheckoutId.HasValue)
        {
            await checkoutOrderRecorder.RecordOrderCreatedAsync(
                notification.SourceCheckoutId.Value,
                notification.OrderId,
                notification.CustomerId,
                cancellationToken);
        }

        var cartResult = await cartRepository.GetByCustomerIdAsync(notification.CustomerId);
        if (cartResult.IsFailure)
        {
            logger.LogWarning(
                "Cart not found while completing checkout for OrderId={OrderId}, CustomerId={CustomerId}",
                notification.OrderId,
                notification.CustomerId);
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var cart = cartResult.Value;
        var completeResult = cart.CompleteCheckout();
        if (completeResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to complete checkout for order {notification.OrderId}: {completeResult.Error}");

        if (!completeResult.Value)
        {
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var updateResult = await cartRepository.UpdateAsync(cart);
        if (updateResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to persist checkout completion for order {notification.OrderId}: {updateResult.Error}");

        inbox.AddProcessed(notification, Consumer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
