using MediatR;
using Microsoft.Extensions.Logging;
using Store.Carts.Contracts.Events;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Domain.Aggregates;
using Store.Ordering.Domain.Interfaces;
using Store.Ordering.Domain.ValueObjects;

namespace Store.Ordering.Application.EventHandlers;

public class CartCheckedOutEventHandler(
    IOrderRepository orderRepository,
    IOrderingUnitOfWork unitOfWork,
    IOrderingDomainEventOutbox outbox,
    IOrderingDomainEventInbox inbox,
    ILogger<CartCheckedOutEventHandler> logger) : INotificationHandler<CartCheckedOutEvent>
{
    private const string Consumer = nameof(CartCheckedOutEventHandler);

    public async Task Handle(CartCheckedOutEvent notification, CancellationToken cancellationToken)
    {
        if (await inbox.HasProcessedAsync(notification.EventId, Consumer, cancellationToken))
            return;

        var existingOrderResult = await orderRepository.GetBySourceCheckoutIdAsync(notification.EventId);
        if (existingOrderResult.IsSuccess)
        {
            inbox.AddProcessed(notification, Consumer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var productResults = notification.Items
            .Select(i => OrderedProduct.Create(i.ProductId, i.ProductName, i.Price, i.Quantity))
            .ToList();

        if (productResults.Any(r => r.IsFailure))
        {
            var error = productResults.First(r => r.IsFailure).Error;
            logger.LogError(
                "Failed to map cart item to ordered product for CartId={CartId}: {Error}",
                notification.CartId, error);
            throw new InvalidOperationException(
                $"Failed to create order from cart {notification.CartId}: {error}");
        }

        var orderResult = Order.Create(
            notification.CustomerId,
            notification.CustomerEmail,
            notification.RecipientName,
            notification.Phone,
            notification.PaymentMethod,
            productResults.Select(r => r.Value).ToList(),
            notification.EventId);

        if (orderResult.IsFailure)
        {
            logger.LogError(
                "Failed to create order for CartId={CartId}, CustomerId={CustomerId}: {Error}",
                notification.CartId, notification.CustomerId, orderResult.Error);
            throw new InvalidOperationException(
                $"Failed to create order from cart {notification.CartId}: {orderResult.Error}");
        }

        var order = orderResult.Value;
        var addResult = await orderRepository.AddAsync(order);
        if (addResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to persist order from cart {notification.CartId}: {addResult.Error}");

        await outbox.AddAsync(order.DomainEvents, cancellationToken);
        inbox.AddProcessed(notification, Consumer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        order.ClearDomainEvents();
    }
}
