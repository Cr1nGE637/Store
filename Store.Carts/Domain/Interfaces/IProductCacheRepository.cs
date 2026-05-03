using CSharpFunctionalExtensions;
using Store.Carts.Domain.ValueObjects;

namespace Store.Carts.Domain.Interfaces;

public interface IProductCacheRepository
{
    Task<Result<ProductInfo>> GetByIdAsync(Guid productId);
    Task<Result> AddAsync(ProductInfo product);
    Task<Result> UpdatePriceAsync(Guid productId, decimal newPrice);
    Task SetUnavailableAsync(Guid productId);
}
