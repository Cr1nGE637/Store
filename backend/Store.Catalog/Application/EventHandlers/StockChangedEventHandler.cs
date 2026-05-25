using MediatR;
using Store.Catalog.Application.Interfaces;
using Store.Inventory.Contracts.Events;

namespace Store.Catalog.Application.EventHandlers;

public class StockChangedEventHandler(
    IProductAvailabilityRepository availabilityRepository,
    ICatalogUnitOfWork unitOfWork) : INotificationHandler<StockChangedEvent>
{
    public async Task Handle(StockChangedEvent notification, CancellationToken cancellationToken)
    {
        await availabilityRepository.UpsertAsync(
            notification.ProductId,
            notification.AvailableQuantity,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
