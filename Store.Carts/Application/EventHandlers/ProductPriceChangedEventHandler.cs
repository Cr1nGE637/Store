using MediatR;
using Store.Catalog.Contracts.Events;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.EventHandlers;

public class ProductPriceChangedEventHandler(
    IProductCacheRepository productCacheRepository,
    ICartUnitOfWork unitOfWork) : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
    {
        var result = await productCacheRepository.UpdatePriceAsync(notification.ProductId, notification.NewPrice);
        if (result.IsSuccess)
            await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
