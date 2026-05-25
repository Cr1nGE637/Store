using CSharpFunctionalExtensions;
using Store.Catalog.Application.CQRS.Command;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.SharedKernel.Events;

namespace Store.Tests.Application;

public class ProductElectronicsCategoryRulesTests
{
    [Fact]
    public async Task CreateProduct_WhenRequiredCategorySpecificationIsMissing_ReturnsFailure()
    {
        var category = Category.Create("Smartphones", "Smartphone").Value;
        var productRepository = new FakeProductRepository();
        var handler = new CreateProductCommandHandler(
            productRepository,
            new FakeCategoryRepository(category),
            new FakeCatalogUnitOfWork(),
            new FakeCatalogDomainEventOutbox());

        var result = await handler.Handle(
            new CreateProductCommand
            {
                Sku = "APL-IP15-128-BLK",
                ProductName = "Apple iPhone 15",
                ProductDescription = "Smartphone",
                ProductPrice = 79990m,
                Brand = "Apple",
                Model = "iPhone 15",
                WarrantyMonths = 12,
                CategoryId = category.CategoryId,
                Specifications = new Dictionary<string, string>
                {
                    ["display"] = "6.1 inch",
                    ["processor"] = "A16 Bionic"
                }
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("memory", result.Error);
        Assert.Empty(productRepository.Products);
    }

    [Fact]
    public async Task CreateProduct_WhenCategorySpecificationSetIsComplete_CreatesProduct()
    {
        var category = Category.Create("Monitors", "Monitor").Value;
        var productRepository = new FakeProductRepository();
        var handler = new CreateProductCommandHandler(
            productRepository,
            new FakeCategoryRepository(category),
            new FakeCatalogUnitOfWork(),
            new FakeCatalogDomainEventOutbox());

        var result = await handler.Handle(
            new CreateProductCommand
            {
                Sku = "LG-27GP850-B",
                ProductName = "LG UltraGear 27GP850-B",
                ProductDescription = "Gaming monitor",
                ProductPrice = 34990m,
                Brand = "LG",
                Model = "27GP850-B",
                WarrantyMonths = 24,
                CategoryId = category.CategoryId,
                Specifications = new Dictionary<string, string>
                {
                    ["display"] = "27 inch",
                    ["resolution"] = "2560x1440",
                    ["refresh_rate"] = "165Hz"
                }
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(productRepository.Products);
    }

    [Fact]
    public async Task UpdateProduct_WhenRequiredCategorySpecificationIsMissing_ReturnsFailureWithoutChangingProduct()
    {
        var category = Category.Create("Smartphones", "Smartphone").Value;
        var product = Product.Create(
            "APL-IP15-128-BLK",
            "Apple iPhone 15",
            "Smartphone",
            79990m,
            "Apple",
            "iPhone 15",
            12,
            category.CategoryId,
            new Dictionary<string, string>
            {
                ["memory"] = "128GB",
                ["display"] = "6.1 inch",
                ["processor"] = "A16 Bionic"
            }).Value;
        var productRepository = new FakeProductRepository(product);
        var handler = new UpdateProductCommandHandler(
            productRepository,
            new FakeCategoryRepository(category),
            new FakeCatalogUnitOfWork(),
            new FakeCatalogDomainEventOutbox());

        var result = await handler.Handle(
            new UpdateProductCommand
            {
                ProductId = product.ProductId,
                Sku = product.Sku.Value,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                ProductPrice = product.ProductPrice.Amount,
                Brand = product.Brand,
                Model = product.Model,
                WarrantyMonths = product.WarrantyMonths,
                CategoryId = category.CategoryId,
                Specifications = new Dictionary<string, string>
                {
                    ["display"] = "6.1 inch",
                    ["processor"] = "A16 Bionic"
                }
            },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("memory", result.Error);
        Assert.Contains(product.Specifications, s => s.Name == "memory" && s.Value == "128GB");
    }

    private sealed class FakeProductRepository(params Product[] products) : IProductRepository
    {
        public List<Product> Products { get; } = products.ToList();

        public Task<Result<List<Product>>> GetAllAsync() => Task.FromResult(Result.Success(Products));

        public Task<Result<List<Product>>> GetPageAsync(int skip, int take) =>
            Task.FromResult(Result.Success(Products.Skip(skip).Take(take).ToList()));

        public Task<Result<List<Product>>> SearchAsync(ProductSearchCriteria criteria) =>
            Task.FromResult(Result.Success(Products));

        public Task<Result<Product>> GetByNameAsync(string name)
        {
            var product = Products.SingleOrDefault(x => x.ProductName == name);
            return Task.FromResult(product is null
                ? Result.Failure<Product>("Product not found")
                : Result.Success(product));
        }

        public Task<Result<Product>> GetBySkuAsync(string sku)
        {
            var product = Products.SingleOrDefault(x => x.Sku.Value == sku);
            return Task.FromResult(product is null
                ? Result.Failure<Product>("Product not found")
                : Result.Success(product));
        }

        public Task<Result<Product>> GetByIdAsync(Guid id)
        {
            var product = Products.SingleOrDefault(x => x.ProductId == id);
            return Task.FromResult(product is null
                ? Result.Failure<Product>("Product not found")
                : Result.Success(product));
        }

        public Task AddAsync(Product product)
        {
            Products.Add(product);
            return Task.CompletedTask;
        }

        public Task<Result> DeleteAsync(Guid id) => Task.FromResult(Result.Success());

        public Task<Result> UpdateAsync(Product product) => Task.FromResult(Result.Success());

        public Task<bool> HasProductsByCategoryAsync(Guid categoryId) => Task.FromResult(false);

        public Task<Result<List<Product>>> GetByCategoryIdAsync(Guid categoryId) =>
            Task.FromResult(Result.Success(Products.Where(p => p.CategoryId == categoryId).ToList()));

        public Task<Result<List<Product>>> GetByCategoryIdAsync(Guid categoryId, int skip, int take) =>
            Task.FromResult(Result.Success(Products.Where(p => p.CategoryId == categoryId).Skip(skip).Take(take).ToList()));
    }

    private sealed class FakeCategoryRepository(params Category[] categories) : ICategoryRepository
    {
        private readonly List<Category> _categories = categories.ToList();

        public Task<Result<List<Category>>> GetAllAsync() => Task.FromResult(Result.Success(_categories));

        public Task<Result<List<Category>>> GetPageAsync(int skip, int take) =>
            Task.FromResult(Result.Success(_categories.Skip(skip).Take(take).ToList()));

        public Task<Result<Category>> GetByIdAsync(Guid id)
        {
            var category = _categories.SingleOrDefault(x => x.CategoryId == id);
            return Task.FromResult(category is null
                ? Result.Failure<Category>("Category not found")
                : Result.Success(category));
        }

        public Task<Result<Category>> GetByNameAsync(string name)
        {
            var category = _categories.SingleOrDefault(x => x.CategoryName == name);
            return Task.FromResult(category is null
                ? Result.Failure<Category>("Category not found")
                : Result.Success(category));
        }

        public Task AddAsync(Category category)
        {
            _categories.Add(category);
            return Task.CompletedTask;
        }

        public Task<Result> UpdateAsync(Category category) => Task.FromResult(Result.Success());

        public Task<Result> DeleteAsync(Guid id) => Task.FromResult(Result.Success());
    }

    private sealed class FakeCatalogUnitOfWork : ICatalogUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(1);
    }

    private sealed class FakeCatalogDomainEventOutbox : ICatalogDomainEventOutbox
    {
        public Task AddAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
