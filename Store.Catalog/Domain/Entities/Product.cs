using CSharpFunctionalExtensions;
using Store.Catalog.Contracts.Events;
using Store.Catalog.Domain.ValueObjects;
using Store.SharedKernel;

namespace Store.Catalog.Domain.Entities;

public class Product : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public Sku Sku { get; private set; }
    public string ProductName { get; private set; }
    public Money ProductPrice { get; private set; }
    public string ProductDescription { get; private set; }
    public string Brand { get; private set; }
    public string Model { get; private set; }
    public int WarrantyMonths { get; private set; }
    public Guid CategoryId { get; private set; }
    public IReadOnlyCollection<ProductSpecification> Specifications => _specifications.AsReadOnly();

    private readonly List<ProductSpecification> _specifications = [];

    private Product(
        Guid productId,
        Sku sku,
        string productName,
        string productDescription,
        Money productPrice,
        string brand,
        string model,
        int warrantyMonths,
        Guid categoryId,
        IEnumerable<ProductSpecification> specifications)
    {
        ProductId = productId;
        Sku = sku;
        ProductName = productName;
        ProductDescription = productDescription;
        ProductPrice = productPrice;
        Brand = brand;
        Model = model;
        WarrantyMonths = warrantyMonths;
        CategoryId = categoryId;
        _specifications.AddRange(specifications);
    }

    public static Result<Product> Create(
        string sku,
        string name,
        string description,
        decimal price,
        string brand,
        string model,
        int warrantyMonths,
        Guid categoryId,
        IReadOnlyDictionary<string, string>? specifications)
    {
        var detailsResult = CreateDetails(sku, name, description, price, brand, model, warrantyMonths, categoryId, specifications);
        if (detailsResult.IsFailure)
            return Result.Failure<Product>(detailsResult.Error);

        var details = detailsResult.Value;
        var product = new Product(
            Guid.NewGuid(),
            details.Sku,
            details.Name,
            details.Description,
            details.Price,
            details.Brand,
            details.Model,
            details.WarrantyMonths,
            details.CategoryId,
            details.Specifications);

        product.RaiseDomainEvent(new ProductCreatedEvent(
            product.ProductId,
            product.Sku.Value,
            product.ProductName,
            product.Brand,
            product.Model,
            product.WarrantyMonths,
            price,
            product.CategoryId));
        return Result.Success(product);
    }

    public Result<bool> Update(
        string sku,
        string name,
        string description,
        decimal price,
        string brand,
        string model,
        int warrantyMonths,
        Guid categoryId,
        IReadOnlyDictionary<string, string>? specifications)
    {
        var detailsResult = CreateDetails(sku, name, description, price, brand, model, warrantyMonths, categoryId, specifications);
        if (detailsResult.IsFailure)
            return Result.Failure<bool>(detailsResult.Error);

        var details = detailsResult.Value;
        if (Sku == details.Sku
            && ProductName == details.Name
            && ProductDescription == details.Description
            && ProductPrice == details.Price
            && Brand == details.Brand
            && Model == details.Model
            && WarrantyMonths == details.WarrantyMonths
            && CategoryId == details.CategoryId
            && Specifications.SequenceEqual(details.Specifications))
            return Result.Success(false);

        if (ProductPrice.Amount != price)
            RaiseDomainEvent(new ProductPriceChangedEvent(ProductId, ProductPrice.Amount, price));

        Sku = details.Sku;
        ProductName = details.Name;
        ProductDescription = details.Description;
        ProductPrice = details.Price;
        Brand = details.Brand;
        Model = details.Model;
        WarrantyMonths = details.WarrantyMonths;
        CategoryId = details.CategoryId;
        _specifications.Clear();
        _specifications.AddRange(details.Specifications);
        return Result.Success(true);
    }

    public void Delete() => RaiseDomainEvent(new ProductDeletedEvent(ProductId, ProductName));

    internal static Product Reconstitute(
        Guid id,
        string sku,
        string name,
        string description,
        decimal price,
        string brand,
        string model,
        int warrantyMonths,
        Guid categoryId,
        IEnumerable<ProductSpecification> specifications) =>
        new(
            id,
            Sku.Reconstitute(sku),
            name,
            description,
            Money.Reconstitute(price),
            brand,
            model,
            warrantyMonths,
            categoryId,
            specifications);

    private static Result<ProductDetails> CreateDetails(
        string sku,
        string name,
        string description,
        decimal price,
        string brand,
        string model,
        int warrantyMonths,
        Guid categoryId,
        IReadOnlyDictionary<string, string>? specifications)
    {
        var skuResult = Sku.Create(sku);
        if (skuResult.IsFailure)
            return Result.Failure<ProductDetails>(skuResult.Error);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ProductDetails>("Product name is required");

        if (string.IsNullOrWhiteSpace(brand))
            return Result.Failure<ProductDetails>("Brand is required");

        if (string.IsNullOrWhiteSpace(model))
            return Result.Failure<ProductDetails>("Model is required");

        if (warrantyMonths < 0)
            return Result.Failure<ProductDetails>("Warranty cannot be negative");

        var normalizedName = name.Trim();
        var normalizedBrand = brand.Trim();
        var normalizedModel = model.Trim();
        var normalizedDescription = description.Trim();

        if (normalizedName.Length > 100)
            return Result.Failure<ProductDetails>("Product name cannot exceed 100 characters");

        if (normalizedDescription.Length > 500)
            return Result.Failure<ProductDetails>("Product description cannot exceed 500 characters");

        if (normalizedBrand.Length > 100)
            return Result.Failure<ProductDetails>("Brand cannot exceed 100 characters");

        if (normalizedModel.Length > 100)
            return Result.Failure<ProductDetails>("Model cannot exceed 100 characters");

        var moneyResult = Money.Create(price);
        if (moneyResult.IsFailure)
            return Result.Failure<ProductDetails>(moneyResult.Error);

        if (categoryId == Guid.Empty)
            return Result.Failure<ProductDetails>("Category is required");

        var specificationResults = (specifications ?? new Dictionary<string, string>())
            .Select(s => ProductSpecification.Create(s.Key, s.Value))
            .ToList();
        if (specificationResults.Any(r => r.IsFailure))
            return Result.Failure<ProductDetails>(specificationResults.First(r => r.IsFailure).Error);

        var normalizedSpecifications = specificationResults
            .Select(r => r.Value)
            .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(s => s.Name)
            .ToList();

        return Result.Success(new ProductDetails(
            skuResult.Value,
            normalizedName,
            normalizedDescription,
            moneyResult.Value,
            normalizedBrand,
            normalizedModel,
            warrantyMonths,
            categoryId,
            normalizedSpecifications));
    }

    private sealed record ProductDetails(
        Sku Sku,
        string Name,
        string Description,
        Money Price,
        string Brand,
        string Model,
        int WarrantyMonths,
        Guid CategoryId,
        IReadOnlyCollection<ProductSpecification> Specifications);
}
