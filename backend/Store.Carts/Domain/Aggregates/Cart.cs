using CSharpFunctionalExtensions;
using Store.Carts.Contracts.Events;
using Store.Carts.Domain.Entities;
using Store.Carts.Domain.ValueObjects;
using Store.SharedKernel;

namespace Store.Carts.Domain.Aggregates;

public class Cart : AggregateRoot
{
    public Guid CartId { get; private set; }
    public Guid CustomerId { get; private set; }
    public bool IsCheckoutPending { get; private set; }
    public Guid? PendingCheckoutId { get; private set; }
    public DateTimeOffset? CheckoutPendingSince { get; private set; }

    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    private Cart(
        Guid cartId,
        Guid customerId,
        bool isCheckoutPending = false,
        Guid? pendingCheckoutId = null,
        DateTimeOffset? checkoutPendingSince = null)
    {
        CartId = cartId;
        CustomerId = customerId;
        IsCheckoutPending = isCheckoutPending;
        PendingCheckoutId = pendingCheckoutId;
        CheckoutPendingSince = checkoutPendingSince;
    }

    public static Result<Cart> Create(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Cart>("CustomerId is required");

        return Result.Success(new Cart(Guid.NewGuid(), customerId));
    }

    internal static Cart Reconstitute(
        Guid id,
        Guid customerId,
        bool isCheckoutPending = false,
        Guid? pendingCheckoutId = null,
        DateTimeOffset? checkoutPendingSince = null) =>
        new(id, customerId, isCheckoutPending, pendingCheckoutId, checkoutPendingSince);

    internal void LoadItems(IEnumerable<CartItem> items) => _items.AddRange(items);

    public Result AddItem(
        Guid productId,
        string productName,
        decimal price,
        int quantity,
        string? mainImageUrl = null,
        string? mainImageAltText = null)
    {
        if (IsCheckoutPending)
            return Result.Failure("Cart checkout is pending");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
            return existing.RefreshProductInfoAndChangeQuantity(
                productName,
                price,
                existing.Quantity + quantity,
                mainImageUrl,
                mainImageAltText);

        var itemResult = CartItem.Create(productId, productName, price, quantity, mainImageUrl, mainImageAltText);
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

    public Result<IReadOnlyList<CartItem>> Checkout(
        string customerEmail,
        string recipientName,
        string phone,
        string deliveryAddress,
        string deliveryMethod,
        string paymentMethod,
        DateTimeOffset? checkoutStartedAt = null)
    {
        if (IsCheckoutPending)
            return Result.Failure<IReadOnlyList<CartItem>>("Cart checkout is pending");

        if (_items.Count == 0)
            return Result.Failure<IReadOnlyList<CartItem>>("Cart is empty");

        if (string.IsNullOrWhiteSpace(customerEmail))
            return Result.Failure<IReadOnlyList<CartItem>>("Customer email is required");
        var checkoutDetailsResult = CheckoutDetails.Create(
            recipientName,
            phone,
            deliveryAddress,
            deliveryMethod,
            paymentMethod);
        if (checkoutDetailsResult.IsFailure)
            return Result.Failure<IReadOnlyList<CartItem>>(checkoutDetailsResult.Error);

        var checkoutDetails = checkoutDetailsResult.Value;

        var snapshot = _items.ToList();

        var eventItems = snapshot
            .Select(i => new CartCheckedOutItem(i.ProductId, i.ProductName, i.Price, i.Quantity))
            .ToList();
        var checkoutEvent = new CartCheckedOutEvent(
            CartId,
            CustomerId,
            customerEmail.Trim(),
            checkoutDetails.RecipientName.Value,
            checkoutDetails.Phone.Value,
            checkoutDetails.DeliveryAddress.Value,
            checkoutDetails.DeliveryMethod.Value,
            checkoutDetails.PaymentMethod.Value,
            eventItems);

        IsCheckoutPending = true;
        PendingCheckoutId = checkoutEvent.EventId;
        CheckoutPendingSince = checkoutStartedAt ?? DateTimeOffset.UtcNow;
        RaiseDomainEvent(checkoutEvent);

        return Result.Success<IReadOnlyList<CartItem>>(snapshot);
    }

    public Result<bool> CompleteCheckout()
    {
        if (!IsCheckoutPending)
            return Result.Success(false);

        _items.Clear();
        IsCheckoutPending = false;
        PendingCheckoutId = null;
        CheckoutPendingSince = null;
        return Result.Success(true);
    }

    public Result<bool> ReleaseCheckout()
    {
        if (!IsCheckoutPending)
            return Result.Success(false);

        IsCheckoutPending = false;
        PendingCheckoutId = null;
        CheckoutPendingSince = null;
        return Result.Success(true);
    }

    public Result<bool> ReleaseStaleCheckout(DateTimeOffset staleBefore, bool orderExistsForCheckout)
    {
        if (!IsCheckoutPending)
            return Result.Success(false);

        if (PendingCheckoutId == null)
            return Result.Failure<bool>("Pending checkout id is missing");

        if (CheckoutPendingSince == null)
            return Result.Failure<bool>("Checkout pending timestamp is missing");

        if (CheckoutPendingSince > staleBefore)
            return Result.Success(false);

        if (orderExistsForCheckout)
            return Result.Success(false);

        return ReleaseCheckout();
    }
}
