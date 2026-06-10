using ClosedXML.Excel;
using CSharpFunctionalExtensions;
using Store.Catalog.Application.CQRS.Command;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Application.Services;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.SharedKernel.Events;

namespace Store.Tests.Application;

public class ImportProductsCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenWorkbookIsValid_CreatesUpdatesAndSetsStock()
    {
        var peripherals = Category.Create("Peripherals", "Peripherals").Value;
        var monitors = Category.Create("Monitors", "Monitors").Value;
        var existingProduct = Product.Create(
            "KB-001",
            "Keyboard",
            "Mechanical keyboard",
            99.9m,
            "Keychron",
            "K8 Pro",
            24,
            peripherals.CategoryId,
            new Dictionary<string, string>
            {
                ["deviceType"] = "Keyboard",
                ["interface"] = "USB-C",
                ["connectorType"] = "USB-C"
            }).Value;
        existingProduct.ClearDomainEvents();

        var productRepository = new FakeProductRepository(existingProduct);
        var unitOfWork = new FakeCatalogUnitOfWork();
        var handler = new ImportProductsCommandHandler(
            productRepository,
            new FakeCategoryRepository(peripherals, monitors),
            unitOfWork,
            new FakeCatalogDomainEventOutbox(),
            new ProductExcelImportPreviewReader());

        await using var workbook = BuildWorkbook(
            products =>
            {
                AddProductRow(products, 2, "KB-001", "Keyboard", "Mechanical keyboard", "Peripherals", "Keychron", "K8 Pro", 24, 109.9m, 5, true);
                AddProductRow(products, 3, "MON-001", "Monitor", "Gaming monitor", "Monitors", "LG", "27GP850-B", 36, 34990m, 3, true);
            },
            specifications =>
            {
                AddSpecificationRow(specifications, 2, "KB-001", "deviceType", "Keyboard");
                AddSpecificationRow(specifications, 3, "KB-001", "interface", "USB-C");
                AddSpecificationRow(specifications, 4, "KB-001", "connectorType", "USB-C");
                AddSpecificationRow(specifications, 5, "MON-001", "screenSize", "27");
                AddSpecificationRow(specifications, 6, "MON-001", "resolution", "2560x1440");
                AddSpecificationRow(specifications, 7, "MON-001", "refreshRate", "165Hz");
                AddSpecificationRow(specifications, 8, "MON-001", "interface", "HDMI");
            });

        var result = await handler.Handle(new ImportProductsCommand(workbook, "products.xlsx"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalRows);
        Assert.Equal(1, result.Value.CreatedCount);
        Assert.Equal(1, result.Value.UpdatedCount);
        Assert.Equal(2, result.Value.StockUpdatedCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(109.9m, existingProduct.ProductPrice.Amount);
        Assert.Contains(productRepository.Products, product => product.Sku.Value == "MON-001");
        Assert.Contains(result.Value.StockUpdates, update => update.AvailableQuantity == 5);
        Assert.Contains(result.Value.StockUpdates, update => update.AvailableQuantity == 3);
    }

    [Fact]
    public async Task Handle_WhenWorkbookHasErrors_ReturnsFailureWithoutSaving()
    {
        var monitors = Category.Create("Monitors", "Monitors").Value;
        var productRepository = new FakeProductRepository();
        var unitOfWork = new FakeCatalogUnitOfWork();
        var handler = new ImportProductsCommandHandler(
            productRepository,
            new FakeCategoryRepository(monitors),
            unitOfWork,
            new FakeCatalogDomainEventOutbox(),
            new ProductExcelImportPreviewReader());

        await using var workbook = BuildWorkbook(
            products =>
            {
                AddProductRow(products, 2, "MON-001", "Monitor", "Gaming monitor", "Monitors", "LG", "27GP850-B", 36, 34990m, 3, true);
                AddProductRow(products, 3, "MON-001", "", "Duplicate monitor", "Unknown", "", "", -1, -10m, -2, true);
            },
            specifications =>
            {
                AddSpecificationRow(specifications, 2, "MON-001", "interface", "HDMI");
            });

        var result = await handler.Handle(new ImportProductsCommand(workbook, "products.xlsx"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("Import file contains errors", result.Error);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Empty(productRepository.Products);
    }

    private static MemoryStream BuildWorkbook(
        Action<IXLWorksheet> products,
        Action<IXLWorksheet> specifications)
    {
        using var workbook = new XLWorkbook();
        var productsSheet = workbook.Worksheets.Add("Products");
        productsSheet.Cell(1, 1).Value = "sku";
        productsSheet.Cell(1, 2).Value = "name";
        productsSheet.Cell(1, 3).Value = "description";
        productsSheet.Cell(1, 4).Value = "categoryCode";
        productsSheet.Cell(1, 5).Value = "brand";
        productsSheet.Cell(1, 6).Value = "model";
        productsSheet.Cell(1, 7).Value = "warrantyMonths";
        productsSheet.Cell(1, 8).Value = "price";
        productsSheet.Cell(1, 9).Value = "stockQuantity";
        productsSheet.Cell(1, 10).Value = "isActive";
        products(productsSheet);

        var specificationsSheet = workbook.Worksheets.Add("Specifications");
        specificationsSheet.Cell(1, 1).Value = "sku";
        specificationsSheet.Cell(1, 2).Value = "key";
        specificationsSheet.Cell(1, 3).Value = "value";
        specificationsSheet.Cell(1, 4).Value = "unit";
        specifications(specificationsSheet);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static void AddProductRow(
        IXLWorksheet sheet,
        int row,
        string sku,
        string name,
        string description,
        string categoryCode,
        string brand,
        string model,
        int warrantyMonths,
        decimal price,
        int stockQuantity,
        bool isActive)
    {
        sheet.Cell(row, 1).Value = sku;
        sheet.Cell(row, 2).Value = name;
        sheet.Cell(row, 3).Value = description;
        sheet.Cell(row, 4).Value = categoryCode;
        sheet.Cell(row, 5).Value = brand;
        sheet.Cell(row, 6).Value = model;
        sheet.Cell(row, 7).Value = warrantyMonths;
        sheet.Cell(row, 8).Value = price;
        sheet.Cell(row, 9).Value = stockQuantity;
        sheet.Cell(row, 10).Value = isActive;
    }

    private static void AddSpecificationRow(IXLWorksheet sheet, int row, string sku, string key, string value)
    {
        sheet.Cell(row, 1).Value = sku;
        sheet.Cell(row, 2).Value = key;
        sheet.Cell(row, 3).Value = value;
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
            var product = Products.SingleOrDefault(product => product.ProductName == name);
            return Task.FromResult(product is null
                ? Result.Failure<Product>("Product not found")
                : Result.Success(product));
        }

        public Task<Result<Product>> GetBySkuAsync(string sku)
        {
            var product = Products.SingleOrDefault(product => product.Sku.Value == sku);
            return Task.FromResult(product is null
                ? Result.Failure<Product>("Product not found")
                : Result.Success(product));
        }

        public Task<Result<Product>> GetByIdAsync(Guid id)
        {
            var product = Products.SingleOrDefault(product => product.ProductId == id);
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
            Task.FromResult(Result.Success(Products.Where(product => product.CategoryId == categoryId).ToList()));

        public Task<Result<List<Product>>> GetByCategoryIdAsync(Guid categoryId, int skip, int take) =>
            Task.FromResult(Result.Success(Products
                .Where(product => product.CategoryId == categoryId)
                .Skip(skip)
                .Take(take)
                .ToList()));
    }

    private sealed class FakeCategoryRepository(params Category[] categories) : ICategoryRepository
    {
        private readonly List<Category> _categories = categories.ToList();

        public Task<Result<List<Category>>> GetAllAsync() => Task.FromResult(Result.Success(_categories));

        public Task<Result<List<Category>>> GetPageAsync(int skip, int take) =>
            Task.FromResult(Result.Success(_categories.Skip(skip).Take(take).ToList()));

        public Task<Result<Category>> GetByIdAsync(Guid id)
        {
            var category = _categories.SingleOrDefault(category => category.CategoryId == id);
            return Task.FromResult(category is null
                ? Result.Failure<Category>("Category not found")
                : Result.Success(category));
        }

        public Task<Result<Category>> GetByNameAsync(string name)
        {
            var category = _categories.SingleOrDefault(category => category.CategoryName == name);
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
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeCatalogDomainEventOutbox : ICatalogDomainEventOutbox
    {
        public Task AddAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

}
