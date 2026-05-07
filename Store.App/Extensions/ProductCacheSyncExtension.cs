using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Store.Carts.Infrastructure.DbContexts;
using Store.Carts.Infrastructure.Entity;
using Store.Catalog.Infrastructure.DbContexts;

namespace Store.App.Extensions;

public static class ProductCacheSyncExtension
{
    public static async Task SyncCartProductCacheAsync(this IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("ProductCacheSync");
        var catalogDb = services.GetRequiredService<CatalogDbContext>();
        var cartDb = services.GetRequiredService<CartDbContext>();

        var products = await catalogDb.Products
            .AsNoTracking()
            .Select(p => new
            {
                p.ProductId,
                p.ProductName,
                p.ProductPrice
            })
            .ToListAsync();

        var productIds = products.Select(p => p.ProductId).ToHashSet();
        var cachedProducts = await cartDb.ProductCache.ToListAsync();
        var cachedById = cachedProducts.ToDictionary(p => p.ProductId);
        var changed = false;

        foreach (var product in products)
        {
            if (cachedById.TryGetValue(product.ProductId, out var cached))
            {
                if (cached.ProductName == product.ProductName
                    && cached.Price == product.ProductPrice
                    && cached.IsAvailable)
                    continue;

                cached.ProductName = product.ProductName;
                cached.Price = product.ProductPrice;
                cached.IsAvailable = true;
                changed = true;
                continue;
            }

            await cartDb.ProductCache.AddAsync(new ProductCacheEntity
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.ProductPrice,
                IsAvailable = true
            });
            changed = true;
        }

        foreach (var cached in cachedProducts.Where(p => !productIds.Contains(p.ProductId) && p.IsAvailable))
        {
            cached.IsAvailable = false;
            changed = true;
        }

        if (!changed)
        {
            logger.LogInformation("Cart product cache is already in sync with catalog");
            return;
        }

        await cartDb.SaveChangesAsync();
        logger.LogInformation("Cart product cache synchronized from catalog: {Count} products", products.Count);
    }
}
