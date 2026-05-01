using CSharpFunctionalExtensions;
using Store.Catalog.Domain.ValueObjects;

namespace Store.Catalog.Domain.Entities;

public class Product
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

        var moneyResult = Money.Create(price);
        if (moneyResult.IsFailure)
            return Result.Failure<Product>(moneyResult.Error);

        if (categoryId == Guid.Empty)
            return Result.Failure<Product>("Category is required");

        return Result.Success(new Product(Guid.NewGuid(), name, description, moneyResult.Value, categoryId));
    }

    public Result Update(string name, string description, decimal price, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Product name is required");

        var moneyResult = Money.Create(price);
        if (moneyResult.IsFailure)
            return Result.Failure(moneyResult.Error);

        if (categoryId == Guid.Empty)
            return Result.Failure("Category is required");

        ProductName = name;
        ProductDescription = description;
        ProductPrice = moneyResult.Value;
        CategoryId = categoryId;
        return Result.Success();
    }

    internal static Product Reconstitute(Guid id, string name, string description, decimal price, Guid categoryId) =>
        new(id, name, description, Money.Create(price).Value, categoryId);
}