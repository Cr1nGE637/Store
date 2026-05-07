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
    public void Checkout_WhenCartHasItems_ReturnsSnapshotClearsCartAndRaisesEvent()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = Cart.Create(customerId).Value;
        cart.AddItem(productId, "Keyboard", 99.9m, 2);

        var result = cart.Checkout("customer@example.com");

        Assert.True(result.IsSuccess);
        var checkedOutItem = Assert.Single(result.Value);
        Assert.Equal(productId, checkedOutItem.ProductId);
        Assert.Empty(cart.Items);

        var domainEvent = Assert.IsType<CartCheckedOutEvent>(Assert.Single(cart.DomainEvents));
        Assert.Equal(cart.CartId, domainEvent.CartId);
        Assert.Equal(customerId, domainEvent.CustomerId);
        Assert.Equal("customer@example.com", domainEvent.CustomerEmail);

        var eventItem = Assert.Single(domainEvent.Items);
        Assert.Equal(productId, eventItem.ProductId);
        Assert.Equal("Keyboard", eventItem.ProductName);
        Assert.Equal(99.9m, eventItem.Price);
        Assert.Equal(2, eventItem.Quantity);
    }

    [Fact]
    public void Checkout_WhenCartIsEmpty_FailsWithoutDomainEvent()
    {
        var cart = Cart.Create(Guid.NewGuid()).Value;

        var result = cart.Checkout("customer@example.com");

        Assert.True(result.IsFailure);
        Assert.Equal("Cart is empty", result.Error);
        Assert.Empty(cart.DomainEvents);
    }
}
