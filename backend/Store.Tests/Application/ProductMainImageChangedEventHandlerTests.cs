using CSharpFunctionalExtensions;
using Store.Catalog.Contracts.Events;
using Store.Carts.Application.EventHandlers;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;
using Store.Carts.Domain.ValueObjects;

namespace Store.Tests.Application;

public class ProductMainImageChangedEventHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductExistsInCache_UpdatesMainImage()
    {
        var productId = Guid.NewGuid();
        var repository = new FakeProductCacheRepository(
            new ProductInfo(productId, "Keyboard", 99.9m));
        var unitOfWork = new FakeCartUnitOfWork();
        var handler = new ProductMainImageChangedEventHandler(repository, unitOfWork);

        await handler.Handle(
            new ProductMainImageChangedEvent(
                productId,
                "/uploads/products/keyboard/image.jpg",
                "Keyboard front view"),
            CancellationToken.None);

        var product = repository.Products[productId];
        Assert.Equal("/uploads/products/keyboard/image.jpg", product.MainImageUrl);
        Assert.Equal("Keyboard front view", product.MainImageAltText);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeProductCacheRepository(params ProductInfo[] products) : IProductCacheRepository
    {
        public Dictionary<Guid, ProductInfo> Products { get; } = products.ToDictionary(product => product.ProductId);

        public Task<Result<ProductInfo>> GetByIdAsync(Guid productId) =>
            Task.FromResult(Products.TryGetValue(productId, out var product)
                ? Result.Success(product)
                : Result.Failure<ProductInfo>("Product not found"));

        public Task<Result> AddAsync(ProductInfo product)
        {
            Products[product.ProductId] = product;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> UpdatePriceAsync(Guid productId, decimal newPrice)
        {
            if (!Products.TryGetValue(productId, out var product))
                return Task.FromResult(Result.Failure("Product not found"));

            Products[productId] = product with { Price = newPrice };
            return Task.FromResult(Result.Success());
        }

        public Task<Result> UpdateMainImageAsync(Guid productId, string imageUrl, string altText)
        {
            if (!Products.TryGetValue(productId, out var product))
                return Task.FromResult(Result.Failure("Product not found"));

            Products[productId] = product with
            {
                MainImageUrl = imageUrl,
                MainImageAltText = altText
            };
            return Task.FromResult(Result.Success());
        }

        public Task SetUnavailableAsync(Guid productId)
        {
            Products.Remove(productId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCartUnitOfWork : ICartUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }
}
