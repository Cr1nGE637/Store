using CSharpFunctionalExtensions;
using Store.Domain.Entities;

namespace Store.Domain.ValueObjects;

public class OrderedProduct : ValueObject
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public string ProductName { get; private set; }
    public decimal Price { get; private set; }
    private OrderedProduct(Guid productId, string productProductName, decimal price, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
        ProductName = productProductName;
        Price = price;
    }

    public static Result<OrderedProduct> Create(Guid productId, int quantity, string productName, decimal price)
    {
        if (productId == Guid.Empty)
            return Result.Failure<OrderedProduct>("Invalid product ID");
        if (string.IsNullOrWhiteSpace(productName))
            return Result.Failure<OrderedProduct>("Product name is required");
        if (price < 0)
            return Result.Failure<OrderedProduct>("Price cannot be negative");
        if (quantity <= 0)
            return Result.Failure<OrderedProduct>("Quantity must be positive");

        return Result.Success(new OrderedProduct(productId, productName, price, quantity));
        
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
       yield return ProductId;
       yield return Quantity;
    }
}