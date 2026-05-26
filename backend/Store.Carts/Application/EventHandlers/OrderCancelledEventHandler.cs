using MediatR;
using Microsoft.Extensions.Logging;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Carts.Application.EventHandlers;

public class OrderCancelledEventHandler(
    ICartRepository cartRepository,
    ICartUnitOfWork unitOfWork,
    ICartDomainEventInbox inbox,
    ILogger<OrderCancelledEventHandler> logger) : INotificationHandler<OrderCancelledEvent>
{
    private const string Consumer = nameof(OrderCancelledEventHandler);

    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        if (!notification.SourceCheckoutId.HasValue)
        {
            logger.LogInformation(
                "OrderId={OrderId} was cancelled without source checkout; cart release skipped",
                notification.OrderId);
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var cartResult = await cartRepository.GetByCustomerIdAsync(notification.CustomerId);
        if (cartResult.IsFailure)
        {
            logger.LogWarning(
                "Cart not found while releasing checkout for cancelled OrderId={OrderId}, CustomerId={CustomerId}",
                notification.OrderId,
                notification.CustomerId);
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var cart = cartResult.Value;
        if (cart.PendingCheckoutId != notification.SourceCheckoutId)
        {
            logger.LogInformation(
                "Checkout release skipped for cancelled OrderId={OrderId}: cart pending checkout does not match SourceCheckoutId={SourceCheckoutId}",
                notification.OrderId,
                notification.SourceCheckoutId);
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var releaseResult = cart.ReleaseCheckout();
        if (releaseResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to release checkout for cancelled order {notification.OrderId}: {releaseResult.Error}");

        if (!releaseResult.Value)
        {
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var updateResult = await cartRepository.UpdateAsync(cart);
        if (updateResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to persist checkout release for cancelled order {notification.OrderId}: {updateResult.Error}");

        inbox.AddProcessed(notification, Consumer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
