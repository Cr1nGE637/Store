using CSharpFunctionalExtensions;

namespace Store.Carts.Domain.Entities;

public class CartItem
{
    public Guid CartItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }

    private CartItem(Guid cartItemId, Guid productId, string productName, decimal price, int quantity)
    {
        CartItemId = cartItemId;
        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }

    internal static Result<CartItem> Create(Guid productId, string productName, decimal price, int quantity)
    {
        if (productId == Guid.Empty)
            return Result.Failure<CartItem>("ProductId is required");

        if (string.IsNullOrWhiteSpace(productName))
            return Result.Failure<CartItem>("Product name is required");

        if (price <= 0)
            return Result.Failure<CartItem>("Price must be greater than zero");

        if (quantity <= 0)
            return Result.Failure<CartItem>("Quantity must be greater than zero");

        return Result.Success(new CartItem(Guid.NewGuid(), productId, productName, price, quantity));
    }

    internal static CartItem Reconstitute(Guid id, Guid productId, string productName, decimal price, int quantity) =>
        new(id, productId, productName, price, quantity);

    internal Result ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure("Quantity must be greater than zero");

        Quantity = quantity;
        return Result.Success();
    }
}
