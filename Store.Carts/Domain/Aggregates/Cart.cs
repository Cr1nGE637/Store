using CSharpFunctionalExtensions;
using Store.Carts.Contracts.Events;
using Store.Carts.Domain.Entities;
using Store.SharedKernel;

namespace Store.Carts.Domain.Aggregates;

public class Cart : AggregateRoot
{
    public Guid CartId { get; private set; }
    public Guid CustomerId { get; private set; }
    public bool IsCheckoutPending { get; private set; }

    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    private Cart(Guid cartId, Guid customerId, bool isCheckoutPending = false)
    {
        CartId = cartId;
        CustomerId = customerId;
        IsCheckoutPending = isCheckoutPending;
    }

    public static Result<Cart> Create(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Cart>("CustomerId is required");

        return Result.Success(new Cart(Guid.NewGuid(), customerId));
    }

    internal static Cart Reconstitute(Guid id, Guid customerId, bool isCheckoutPending = false) =>
        new(id, customerId, isCheckoutPending);

    internal void LoadItems(IEnumerable<CartItem> items) => _items.AddRange(items);

    public Result AddItem(Guid productId, string productName, decimal price, int quantity)
    {
        if (IsCheckoutPending)
            return Result.Failure("Cart checkout is pending");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
            return existing.RefreshProductInfoAndChangeQuantity(productName, price, existing.Quantity + quantity);

        var itemResult = CartItem.Create(productId, productName, price, quantity);
        if (itemResult.IsFailure)
            return Result.Failure(itemResult.Error);

        _items.Add(itemResult.Value);
        return Result.Success();
    }

    public Result RemoveItem(Guid cartItemId)
    {
        if (IsCheckoutPending)
            return Result.Failure("Cart checkout is pending");

        var item = _items.FirstOrDefault(i => i.CartItemId == cartItemId);
        if (item == null)
            return Result.Failure("Cart item not found");

        _items.Remove(item);
        return Result.Success();
    }

    public Result ChangeQuantity(Guid cartItemId, int quantity)
    {
        if (IsCheckoutPending)
            return Result.Failure("Cart checkout is pending");

        var item = _items.FirstOrDefault(i => i.CartItemId == cartItemId);
        if (item == null)
            return Result.Failure("Cart item not found");

        return item.ChangeQuantity(quantity);
    }

    public Result<IReadOnlyList<CartItem>> Checkout(string customerEmail)
    {
        if (IsCheckoutPending)
            return Result.Failure<IReadOnlyList<CartItem>>("Cart checkout is pending");

        if (_items.Count == 0)
            return Result.Failure<IReadOnlyList<CartItem>>("Cart is empty");

        var snapshot = _items.ToList();
        IsCheckoutPending = true;

        var eventItems = snapshot
            .Select(i => new CartCheckedOutItem(i.ProductId, i.ProductName, i.Price, i.Quantity))
            .ToList();
        RaiseDomainEvent(new CartCheckedOutEvent(CartId, CustomerId, customerEmail, eventItems));

        return Result.Success<IReadOnlyList<CartItem>>(snapshot);
    }

    public Result<bool> CompleteCheckout()
    {
        if (!IsCheckoutPending)
            return Result.Success(false);

        _items.Clear();
        IsCheckoutPending = false;
        return Result.Success(true);
    }

    public Result<bool> ReleaseCheckout()
    {
        if (!IsCheckoutPending)
            return Result.Success(false);

        IsCheckoutPending = false;
        return Result.Success(true);
    }
}
