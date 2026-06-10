using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Consulting.Application.Interfaces;
using Store.Consulting.Domain.ValueObjects;
using Store.Consulting.Infrastructure.DbContexts;
using Store.Consulting.Infrastructure.Entity;

namespace Store.Consulting.Infrastructure.Repository;

public sealed class ConsultingProductReadRepository(ConsultingCatalogReadDbContext dbContext)
    : IConsultingProductReadRepository
{
    public async Task<Result<ConsultationProduct>> GetByIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        if (productId == Guid.Empty)
            return Result.Failure<ConsultationProduct>("Product id is required");

        var product = await QueryProducts()
            .FirstOrDefaultAsync(entity => entity.ProductId == productId, cancellationToken);

        return product is null
            ? Result.Failure<ConsultationProduct>("Product not found")
            : Result.Success(ToConsultationProduct(product));
    }

    public async Task<Result<IReadOnlyCollection<ConsultationProduct>>> GetByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        var requestedIds = productIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (requestedIds.Length == 0)
            return Result.Success<IReadOnlyCollection<ConsultationProduct>>([]);

        var products = await QueryProducts()
            .Where(product => requestedIds.Contains(product.ProductId))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyCollection<ConsultationProduct>>(
            products.Select(ToConsultationProduct).ToArray());
    }

    public async Task<Result<IReadOnlyCollection<ConsultationProduct>>> GetByCategoryCodeAsync(
        string categoryCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(categoryCode))
            return Result.Failure<IReadOnlyCollection<ConsultationProduct>>("Category code is required");

        var products = await QueryProducts()
            .Where(product => product.Category.CategoryCode == categoryCode.Trim())
            .OrderBy(product => product.ProductName)
            .Take(20)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyCollection<ConsultationProduct>>(
            products.Select(ToConsultationProduct).ToArray());
    }

    private IQueryable<ConsultingCatalogProductEntity> QueryProducts() =>
        dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Specifications);

    private static ConsultationProduct ToConsultationProduct(ConsultingCatalogProductEntity product) =>
        new(
            product.ProductId,
            product.ProductName,
            product.Category.CategoryCode,
            product.Specifications.ToDictionary(
                specification => specification.Name,
                specification => specification.Value,
                StringComparer.OrdinalIgnoreCase));
}
