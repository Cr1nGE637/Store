using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Carts.Domain.Interfaces;
using Store.Carts.Domain.ValueObjects;
using Store.Carts.Infrastructure.DbContexts;
using Store.Carts.Infrastructure.Entity;

namespace Store.Carts.Infrastructure.Repository;

public class ProductCacheRepository(CartDbContext context) : IProductCacheRepository
{
    public async Task<Result<ProductInfo>> GetByIdAsync(Guid productId)
    {
        var entity = await context.ProductCache
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (entity == null)
            return Result.Failure<ProductInfo>("Product not found");

        if (!entity.IsAvailable)
            return Result.Failure<ProductInfo>("Product is not available");

        return Result.Success(new ProductInfo(entity.ProductId, entity.ProductName, entity.Price));
    }

    public async Task<Result> AddAsync(ProductInfo product)
    {
        var exists = await context.ProductCache.AnyAsync(p => p.ProductId == product.ProductId);
        if (exists)
            return Result.Success();

        await context.ProductCache.AddAsync(new ProductCacheEntity
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Price = product.Price,
            IsAvailable = true
        });
        return Result.Success();
    }

    public async Task<Result> UpdatePriceAsync(Guid productId, decimal newPrice)
    {
        var entity = await context.ProductCache
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (entity == null)
            return Result.Failure("Product not found in cache");

        entity.Price = newPrice;
        return Result.Success();
    }

    public async Task SetUnavailableAsync(Guid productId)
    {
        var entity = await context.ProductCache
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (entity != null)
            entity.IsAvailable = false;
    }
}
