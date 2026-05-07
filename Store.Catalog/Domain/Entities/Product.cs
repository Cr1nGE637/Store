using CSharpFunctionalExtensions;
using Store.Catalog.Contracts.Events;
using Store.Catalog.Domain.ValueObjects;
using Store.SharedKernel;

namespace Store.Catalog.Domain.Entities;

public class Product : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public Money ProductPrice { get; private set; }
    public string ProductDescription { get; private set; }
    public Guid CategoryId { get; private set; }

    private Product(Guid productId, string productName, string productDescription, Money productPrice, Guid categoryId)
    {
        ProductId = productId;
        ProductName = productName;
        ProductDescription = productDescription;
        ProductPrice = productPrice;
        CategoryId = categoryId;
    }

    public static Result<Product> Create(string name, string description, decimal price, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>("Product name is required");

        var normalizedName = name.Trim();
        var moneyResult = Money.Create(price);
        if (moneyResult.IsFailure)
            return Result.Failure<Product>(moneyResult.Error);

        if (categoryId == Guid.Empty)
            return Result.Failure<Product>("Category is required");

        var product = new Product(Guid.NewGuid(), normalizedName, description, moneyResult.Value, categoryId);
        product.RaiseDomainEvent(new ProductCreatedEvent(product.ProductId, product.ProductName, price, product.CategoryId));
        return Result.Success(product);
    }

    public Result<bool> Update(string name, string description, decimal price, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<bool>("Product name is required");

        var normalizedName = name.Trim();
        var moneyResult = Money.Create(price);
        if (moneyResult.IsFailure)
            return Result.Failure<bool>(moneyResult.Error);

        if (categoryId == Guid.Empty)
            return Result.Failure<bool>("Category is required");

        if (ProductName == normalizedName
            && ProductDescription == description
            && ProductPrice.Amount == price
            && CategoryId == categoryId)
            return Result.Success(false);

        if (ProductPrice.Amount != price)
            RaiseDomainEvent(new ProductPriceChangedEvent(ProductId, ProductPrice.Amount, price));

        ProductName = normalizedName;
        ProductDescription = description;
        ProductPrice = moneyResult.Value;
        CategoryId = categoryId;
        return Result.Success(true);
    }

    public void Delete() => RaiseDomainEvent(new ProductDeletedEvent(ProductId, ProductName));

    internal static Product Reconstitute(Guid id, string name, string description, decimal price, Guid categoryId) =>
        new(id, name, description, Money.Reconstitute(price), categoryId);
}
