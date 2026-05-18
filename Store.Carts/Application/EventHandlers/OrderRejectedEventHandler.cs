using MediatR;
using Microsoft.Extensions.Logging;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;
using Store.Ordering.Contracts.Events;

namespace Store.Carts.Application.EventHandlers;

public class OrderRejectedEventHandler(
    ICartRepository cartRepository,
    ICartUnitOfWork unitOfWork,
    ILogger<OrderRejectedEventHandler> logger) : INotificationHandler<OrderRejectedEvent>
{
    public async Task Handle(OrderRejectedEvent notification, CancellationToken cancellationToken)
    {
        var cartResult = await cartRepository.GetByCustomerIdAsync(notification.CustomerId);
        if (cartResult.IsFailure)
        {
            logger.LogWarning(
                "Cart not found while releasing checkout for rejected OrderId={OrderId}, CustomerId={CustomerId}",
                notification.OrderId,
                notification.CustomerId);
            return;
        }

        var cart = cartResult.Value;
        var releaseResult = cart.ReleaseCheckout();
        if (releaseResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to release checkout for rejected order {notification.OrderId}: {releaseResult.Error}");

        if (!releaseResult.Value)
            return;

        var updateResult = await cartRepository.UpdateAsync(cart);
        if (updateResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to persist checkout release for rejected order {notification.OrderId}: {updateResult.Error}");

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
