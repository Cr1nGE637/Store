using MediatR;
using Store.Catalog.Contracts.Events;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.EventHandlers;

public sealed class ProductMainImageChangedEventHandler(
    IProductCacheRepository productCacheRepository,
    ICartUnitOfWork unitOfWork) : INotificationHandler<ProductMainImageChangedEvent>
{
    public async Task Handle(ProductMainImageChangedEvent notification, CancellationToken cancellationToken)
    {
        var result = await productCacheRepository.UpdateMainImageAsync(
            notification.ProductId,
            notification.ImageUrl,
            notification.AltText);

        if (result.IsSuccess)
            await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
