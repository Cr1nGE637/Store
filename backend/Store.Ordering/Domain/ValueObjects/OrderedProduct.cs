using CSharpFunctionalExtensions;

namespace Store.Ordering.Domain.ValueObjects;

public class OrderedProduct : ValueObject
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public decimal Price { get; }
    public int Quantity { get; }

    private OrderedProduct(Guid productId, string productName, decimal price, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }

    public static Result<OrderedProduct> Create(Guid productId, string productName, decimal price, int quantity)
    {
        if (productId == Guid.Empty)
            return Result.Failure<OrderedProduct>("ProductId is required");
        if (string.IsNullOrWhiteSpace(productName))
            return Result.Failure<OrderedProduct>("ProductName is required");
        if (price < 0)
            return Result.Failure<OrderedProduct>("Price cannot be negative");
        if (quantity <= 0)
            return Result.Failure<OrderedProduct>("Quantity must be positive");

        return Result.Success(new OrderedProduct(productId, productName, price, quantity));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return ProductName;
        yield return Price;
        yield return Quantity;
    }
}
