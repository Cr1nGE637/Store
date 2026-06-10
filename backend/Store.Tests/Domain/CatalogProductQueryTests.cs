using CSharpFunctionalExtensions;
using Store.Catalog.Application.CQRS.Query;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Tests.Domain;

public class CatalogProductQueryTests
{
    [Fact]
    public async Task GetProductsQueryHandler_PassesElectronicsFiltersToRepository()
    {
        var repository = new CapturingProductRepository();
        var handler = new GetProductsQueryHandler(
            repository,
            new FakeProductAvailabilityRepository(),
            new FakeProductImageRepository());

        var result = await handler.Handle(
            new GetProductsQuery
            {
                Search = "thinkpad",
                CategoryId = Guid.NewGuid(),
                Brand = "Lenovo",
                MinPrice = 50000m,
                MaxPrice = 150000m,
                InStockOnly = true,
                Page = 2,
                PageSize = 10,
                SpecificationFilters = new Dictionary<string, string> { [" form_factor "] = " ATX " }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(repository.LastCriteria);
        Assert.Equal("thinkpad", repository.LastCriteria.Search);
        Assert.Equal("Lenovo", repository.LastCriteria.Brand);
        Assert.Equal(50000m, repository.LastCriteria.MinPrice);
        Assert.Equal(150000m, repository.LastCriteria.MaxPrice);
        Assert.True(repository.LastCriteria.InStockOnly);
        Assert.Equal(10, repository.LastCriteria.Skip);
        Assert.Equal(10, repository.LastCriteria.Take);
        Assert.Equal("ATX", repository.LastCriteria.SpecificationFilters["formFactor"]);
    }

    [Fact]
    public async Task GetProductsQueryHandler_WhenMinPriceIsGreaterThanMaxPrice_ReturnsFailure()
    {
        var handler = new GetProductsQueryHandler(
            new CapturingProductRepository(),
            new FakeProductAvailabilityRepository(),
            new FakeProductImageRepository());

        var result = await handler.Handle(
            new GetProductsQuery { MinPrice = 100m, MaxPrice = 50m },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Minimum price cannot be greater than maximum price", result.Error);
    }

    [Fact]
    public async Task GetProductsQueryHandler_WhenSpecificationFiltersContainEmptyValues_IgnoresThem()
    {
        var repository = new CapturingProductRepository();
        var handler = new GetProductsQueryHandler(
            repository,
            new FakeProductAvailabilityRepository(),
            new FakeProductImageRepository());

        var result = await handler.Handle(
            new GetProductsQuery
            {
                SpecificationFilters = new Dictionary<string, string>
                {
                    [""] = "ignored",
                    ["memoryType"] = "",
                    ["socket"] = null!,
                    ["PowerConsumptionWatts"] = "  65  "
                }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(repository.LastCriteria);
        Assert.Single(repository.LastCriteria.SpecificationFilters);
        Assert.Equal("65", repository.LastCriteria.SpecificationFilters["powerConsumptionWatts"]);
    }

    private sealed class FakeProductAvailabilityRepository : IProductAvailabilityRepository
    {
        public Task<IReadOnlyDictionary<Guid, int>> GetAvailableQuantitiesAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<Guid, int>>(new Dictionary<Guid, int>());

        public Task UpsertAsync(Guid productId, int availableQuantity, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeProductImageRepository : IProductImageRepository
    {
        public Task AddAsync(ProductImage image, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task ClearMainImageAsync(Guid productId, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<int> GetNextDisplayOrderAsync(Guid productId, CancellationToken cancellationToken) =>
            Task.FromResult(0);

        public Task<Result<ProductImageDto>> GetByIdAsync(Guid productImageId, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Failure<ProductImageDto>("Product image not found"));

        public Task<IReadOnlyCollection<ProductImageDto>> GetByProductIdAsync(
            Guid productId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<ProductImageDto>>([]);

        public Task<Result<ProductImageDto>> DeleteAsync(
            Guid productId,
            Guid productImageId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Result.Failure<ProductImageDto>("Product image not found"));

        public Task<Result<ProductImageDto>> SetMainAsync(
            Guid productId,
            Guid productImageId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Result.Failure<ProductImageDto>("Product image not found"));

        public Task<Result<ProductImageDto?>> SetFirstAvailableAsMainAsync(
            Guid productId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success<ProductImageDto?>(null));

        public Task<IReadOnlyDictionary<Guid, ProductImageDto>> GetMainImagesAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<Guid, ProductImageDto>>(new Dictionary<Guid, ProductImageDto>());
    }

    private sealed class CapturingProductRepository : IProductRepository
    {
        public ProductSearchCriteria? LastCriteria { get; private set; }

        public Task<Result<List<Product>>> SearchAsync(ProductSearchCriteria criteria)
        {
            LastCriteria = criteria;
            return Task.FromResult(Result.Success(new List<Product>()));
        }

        public Task<Result<List<Product>>> GetAllAsync() => Task.FromResult(Result.Success(new List<Product>()));

        public Task<Result<List<Product>>> GetPageAsync(int skip, int take) =>
            Task.FromResult(Result.Success(new List<Product>()));

        public Task<Result<Product>> GetByNameAsync(string name) =>
            Task.FromResult(Result.Failure<Product>("Product not found"));

        public Task<Result<Product>> GetBySkuAsync(string sku) =>
            Task.FromResult(Result.Failure<Product>("Product not found"));

        public Task<Result<Product>> GetByIdAsync(Guid id) =>
            Task.FromResult(Result.Failure<Product>("Product not found"));

        public Task AddAsync(Product product) => Task.CompletedTask;

        public Task<Result> DeleteAsync(Guid id) => Task.FromResult(Result.Success());

        public Task<Result> UpdateAsync(Product product) => Task.FromResult(Result.Success());

        public Task<bool> HasProductsByCategoryAsync(Guid categoryId) => Task.FromResult(false);

        public Task<Result<List<Product>>> GetByCategoryIdAsync(Guid categoryId) =>
            Task.FromResult(Result.Success(new List<Product>()));

        public Task<Result<List<Product>>> GetByCategoryIdAsync(Guid categoryId, int skip, int take) =>
            Task.FromResult(Result.Success(new List<Product>()));
    }
}
