using CSharpFunctionalExtensions;
using Store.Catalog.Application.CQRS.Query;
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
        var handler = new GetProductsQueryHandler(repository, new FakeProductAvailabilityRepository());

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
                SpecificationFilters = new Dictionary<string, string> { ["Processor"] = "Ryzen 7" }
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
        Assert.Equal("Ryzen 7", repository.LastCriteria.SpecificationFilters["Processor"]);
    }

    [Fact]
    public async Task GetProductsQueryHandler_WhenMinPriceIsGreaterThanMaxPrice_ReturnsFailure()
    {
        var handler = new GetProductsQueryHandler(
            new CapturingProductRepository(),
            new FakeProductAvailabilityRepository());

        var result = await handler.Handle(
            new GetProductsQuery { MinPrice = 100m, MaxPrice = 50m },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Minimum price cannot be greater than maximum price", result.Error);
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
