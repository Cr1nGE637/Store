using Microsoft.EntityFrameworkCore;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Infrastructure.DbContexts;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.Repository;

public class ProductAvailabilityRepository(CatalogDbContext dbContext) : IProductAvailabilityRepository
{
    public async Task<IReadOnlyDictionary<Guid, int>> GetAvailableQuantitiesAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return new Dictionary<Guid, int>();

        return await dbContext.ProductAvailabilities
            .AsNoTracking()
            .Where(availability => productIds.Contains(availability.ProductId))
            .ToDictionaryAsync(
                availability => availability.ProductId,
                availability => availability.AvailableQuantity,
                cancellationToken);
    }

    public async Task UpsertAsync(Guid productId, int availableQuantity, CancellationToken cancellationToken)
    {
        var productExists = await dbContext.Products
            .AnyAsync(product => product.ProductId == productId, cancellationToken);
        if (!productExists)
            return;

        var entity = await dbContext.ProductAvailabilities
            .FirstOrDefaultAsync(availability => availability.ProductId == productId, cancellationToken);

        if (entity is null)
        {
            await dbContext.ProductAvailabilities.AddAsync(new ProductAvailabilityEntity
            {
                ProductId = productId,
                AvailableQuantity = availableQuantity,
                UpdatedOnUtc = DateTime.UtcNow
            }, cancellationToken);

            return;
        }

        entity.AvailableQuantity = availableQuantity;
        entity.UpdatedOnUtc = DateTime.UtcNow;
    }
}
