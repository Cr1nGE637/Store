using CSharpFunctionalExtensions;
using Store.Catalog.Domain.ValueObjects;

namespace Store.Catalog.Domain.Entities;

public class Product
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public Money ProductPrice { get; private set; }
    public string ProductDescription { get; private set; }

    private Product(Guid productId, string productName, string productDescription, Money productPrice)
    {
        ProductId = productId;
        ProductName = productName;
        ProductDescription = productDescription;
        ProductPrice = productPrice;
    }

    public static Result<Product> Create(string name, string description, decimal price, Guid id = default)
    {
        if (id == Guid.Empty)
            id = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>("Product name is required");

        var moneyResult = Money.Create(price);
        if (moneyResult.IsFailure)
            return Result.Failure<Product>(moneyResult.Error);

        return Result.Success(new Product(id, name, description, moneyResult.Value));
    }
}