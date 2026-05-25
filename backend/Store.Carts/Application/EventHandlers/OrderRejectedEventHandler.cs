using MediatR;
using Microsoft.Extensions.Logging;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Carts.Application.EventHandlers;

public class OrderRejectedEventHandler(
    ICartRepository cartRepository,
    ICartUnitOfWork unitOfWork,
    ICartDomainEventInbox inbox,
    ILogger<OrderRejectedEventHandler> logger) : INotificationHandler<OrderRejectedEvent>
{
    private const string Consumer = nameof(OrderRejectedEventHandler);

    public async Task Handle(OrderRejectedEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        var cartResult = await cartRepository.GetByCustomerIdAsync(notification.CustomerId);
        if (cartResult.IsFailure)
        {
            logger.LogWarning(
                "Cart not found while releasing checkout for rejected OrderId={OrderId}, CustomerId={CustomerId}",
                notification.OrderId,
                notification.CustomerId);
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var cart = cartResult.Value;
        var releaseResult = cart.ReleaseCheckout();
        if (releaseResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to release checkout for rejected order {notification.OrderId}: {releaseResult.Error}");

        if (!releaseResult.Value)
        {
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var updateResult = await cartRepository.UpdateAsync(cart);
        if (updateResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to persist checkout release for rejected order {notification.OrderId}: {updateResult.Error}");

        inbox.AddProcessed(notification, Consumer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
