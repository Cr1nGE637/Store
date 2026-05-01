using MediatR;
using Store.Catalog.Contracts.Events;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.EventHandlers;

public class ProductDeletedEventHandler(
    IProductCacheRepository productCacheRepository,
    ICartUnitOfWork unitOfWork) : INotificationHandler<ProductDeletedEvent>
{
    public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        await productCacheRepository.SetUnavailableAsync(notification.ProductId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
