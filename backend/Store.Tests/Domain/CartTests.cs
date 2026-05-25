using Store.Carts.Contracts.Events;
using Store.Carts.Domain.Aggregates;

namespace Store.Tests.Domain;

public class CartTests
{
    [Fact]
    public void AddItem_WhenProductAlreadyExists_RefreshesProductInfoAndIncrementsQuantity()
    {
        var cart = Cart.Create(Guid.NewGuid()).Value;
        var productId = Guid.NewGuid();

        cart.AddItem(productId, "Old name", 10m, 1);
        var result = cart.AddItem(productId, "New name", 12.5m, 2);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(cart.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal("New name", item.ProductName);
        Assert.Equal(12.5m, item.Price);
        Assert.Equal(3, item.Quantity);
    }

    [Fact]
    public void Checkout_WhenCartHasItems_ReturnsSnapshotKeepsItemsPendingAndRaisesEvent()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = Cart.Create(customerId).Value;
        cart.AddItem(productId, "Keyboard", 99.9m, 2);

        var result = Checkout(cart);

        Assert.True(result.IsSuccess);
        var checkedOutItem = Assert.Single(result.Value);
        Assert.Equal(productId, checkedOutItem.ProductId);
        Assert.Single(cart.Items);
        Assert.True(cart.IsCheckoutPending);
        Assert.NotNull(cart.PendingCheckoutId);
        Assert.NotNull(cart.CheckoutPendingSince);

        var domainEvent = Assert.IsType<CartCheckedOutEvent>(Assert.Single(cart.DomainEvents));
        Assert.Equal(cart.PendingCheckoutId, domainEvent.EventId);
        Assert.Equal(cart.CartId, domainEvent.CartId);
        Assert.Equal(customerId, domainEvent.CustomerId);
        Assert.Equal("customer@example.com", domainEvent.CustomerEmail);
        Assert.Equal("Ivan Petrov", domainEvent.RecipientName);
        Assert.Equal("+79990000000", domainEvent.Phone);
        Assert.Equal("Tomsk, Lenina 1", domainEvent.DeliveryAddress);
        Assert.Equal("Courier", domainEvent.DeliveryMethod);
        Assert.Equal("Card", domainEvent.PaymentMethod);

        var eventItem = Assert.Single(domainEvent.Items);
        Assert.Equal(productId, eventItem.ProductId);
        Assert.Equal("Keyboard", eventItem.ProductName);
        Assert.Equal(99.9m, eventItem.Price);
        Assert.Equal(2, eventItem.Quantity);
    }

    [Fact]
    public void AddItem_WhenCheckoutIsPending_FailsWithoutChangingSnapshot()
    {
        var cart = Cart.Create(Guid.NewGuid()).Value;
        var productId = Guid.NewGuid();
        cart.AddItem(productId, "Keyboard", 99.9m, 2);
        Checkout(cart);

        var result = cart.AddItem(Guid.NewGuid(), "Mouse", 49.9m, 1);

        Assert.True(result.IsFailure);
        Assert.Equal("Cart checkout is pending", result.Error);
        var item = Assert.Single(cart.Items);
        Assert.Equal(productId, item.ProductId);
    }

    [Fact]
    public void CompleteCheckout_WhenCheckoutIsPending_ClearsCartAndUnlocksIt()
    {
        var cart = Cart.Create(Guid.NewGuid()).Value;
        cart.AddItem(Guid.NewGuid(), "Keyboard", 99.9m, 2);
        Checkout(cart);

        var result = cart.CompleteCheckout();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Empty(cart.Items);
        Assert.False(cart.IsCheckoutPending);
        Assert.Null(cart.PendingCheckoutId);
        Assert.Null(cart.CheckoutPendingSince);
    }

    [Fact]
    public void ReleaseCheckout_WhenCheckoutIsPending_KeepsItemsAndUnlocksCart()
    {
        var cart = Cart.Create(Guid.NewGuid()).Value;
        var productId = Guid.NewGuid();
        cart.AddItem(productId, "Keyboard", 99.9m, 2);
        Checkout(cart);

        var result = cart.ReleaseCheckout();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        var item = Assert.Single(cart.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.False(cart.IsCheckoutPending);
        Assert.Null(cart.PendingCheckoutId);
        Assert.Null(cart.CheckoutPendingSince);
    }

    [Fact]
    public void Checkout_WhenCartIsEmpty_FailsWithoutDomainEvent()
    {
        var cart = Cart.Create(Guid.NewGuid()).Value;

        var result = cart.Checkout(
            "customer@example.com",
            "Ivan Petrov",
            "+79990000000",
            "Tomsk, Lenina 1",
            "Courier",
            "Card");

        Assert.True(result.IsFailure);
        Assert.Equal("Cart is empty", result.Error);
        Assert.Empty(cart.DomainEvents);
    }

    private static CSharpFunctionalExtensions.Result<IReadOnlyList<Store.Carts.Domain.Entities.CartItem>> Checkout(Cart cart) =>
        cart.Checkout(
            "customer@example.com",
            "Ivan Petrov",
            "+79990000000",
            "Tomsk, Lenina 1",
            "Courier",
            "Card");
}
