namespace Store.Catalog.Application.Interfaces;

public interface IProductAvailabilityRepository
{
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableQuantitiesAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);

    Task UpsertAsync(Guid productId, int availableQuantity, CancellationToken cancellationToken);
}
