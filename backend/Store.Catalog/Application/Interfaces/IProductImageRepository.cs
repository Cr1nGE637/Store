using CSharpFunctionalExtensions;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;

namespace Store.Catalog.Application.Interfaces;

public interface IProductImageRepository
{
    Task AddAsync(ProductImage image, CancellationToken cancellationToken);
    Task ClearMainImageAsync(Guid productId, CancellationToken cancellationToken);
    Task<int> GetNextDisplayOrderAsync(Guid productId, CancellationToken cancellationToken);
    Task<Result<ProductImageDto>> GetByIdAsync(Guid productImageId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProductImageDto>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    Task<Result<ProductImageDto>> DeleteAsync(Guid productId, Guid productImageId, CancellationToken cancellationToken);
    Task<Result<ProductImageDto>> SetMainAsync(Guid productId, Guid productImageId, CancellationToken cancellationToken);
    Task<Result<ProductImageDto?>> SetFirstAvailableAsMainAsync(Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, ProductImageDto>> GetMainImagesAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);
}
