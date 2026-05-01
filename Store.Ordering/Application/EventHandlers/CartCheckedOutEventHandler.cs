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
    IPublisher publisher,
    ILogger<CartCheckedOutEventHandler> logger) : INotificationHandler<CartCheckedOutEvent>
{
    public async Task Handle(CartCheckedOutEvent notification, CancellationToken cancellationToken)
    {
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
            productResults.Select(r => r.Value).ToList());

        if (orderResult.IsFailure)
        {
            logger.LogError(
                "Failed to create order for CartId={CartId}, CustomerId={CustomerId}: {Error}",
                notification.CartId, notification.CustomerId, orderResult.Error);
            throw new InvalidOperationException(
                $"Failed to create order from cart {notification.CartId}: {orderResult.Error}");
        }

        var order = orderResult.Value;
        await orderRepository.AddAsync(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in order.DomainEvents)
            await publisher.Publish(domainEvent, cancellationToken);
    }
}
