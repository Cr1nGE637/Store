using MediatR;
using Store.Catalog.Contracts.Events;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;
using Store.Carts.Domain.ValueObjects;

namespace Store.Carts.Application.EventHandlers;

public class ProductCreatedEventHandler(
    IProductCacheRepository productCacheRepository,
    ICartUnitOfWork unitOfWork) : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        var product = new ProductInfo(notification.ProductId, notification.ProductName, notification.Price);
        await productCacheRepository.AddAsync(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
