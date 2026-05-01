using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Carts.Domain.Interfaces;
using Store.Carts.Domain.ValueObjects;
using Store.Carts.Infrastructure.DbContexts;
using Store.Carts.Infrastructure.Entities;

namespace Store.Carts.Infrastructure.Repository;

public class ProductCacheRepository(CartDbContext context) : IProductCacheRepository
{
    public async Task<Result<ProductInfo>> GetByIdAsync(Guid productId)
    {
        var entity = await context.ProductCache
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (entity == null)
            return Result.Failure<ProductInfo>("Product not found");

        if (!entity.IsAvailable)
            return Result.Failure<ProductInfo>("Product is not available");

        return Result.Success(new ProductInfo(entity.ProductId, entity.ProductName, entity.Price));
    }

    public async Task AddAsync(ProductInfo product)
    {
        await context.ProductCache.AddAsync(new ProductCacheEntity
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Price = product.Price,
            IsAvailable = true
        });
    }

    public async Task SetUnavailableAsync(Guid productId)
    {
        var entity = await context.ProductCache
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (entity != null)
            entity.IsAvailable = false;
    }
}
