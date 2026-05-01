using CSharpFunctionalExtensions;
using Store.Carts.Domain.ValueObjects;

namespace Store.Carts.Domain.Interfaces;

public interface IProductCacheRepository
{
    Task<Result<ProductInfo>> GetByIdAsync(Guid productId);
    Task AddAsync(ProductInfo product);
    Task SetUnavailableAsync(Guid productId);
}
