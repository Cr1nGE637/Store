using Store.Catalog.Contracts.Events;
using Store.Catalog.Domain.Entities;

namespace Store.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_TrimsNameAndRaisesCreatedEvent()
    {
        var categoryId = Guid.NewGuid();

        var result = Product.Create("  Keyboard  ", "Mechanical keyboard", 99.9m, categoryId);

        Assert.True(result.IsSuccess);
        var product = result.Value;
        Assert.Equal("Keyboard", product.ProductName);

        var domainEvent = Assert.IsType<ProductCreatedEvent>(Assert.Single(product.DomainEvents));
        Assert.Equal(product.ProductId, domainEvent.ProductId);
        Assert.Equal("Keyboard", domainEvent.ProductName);
        Assert.Equal(99.9m, domainEvent.Price);
        Assert.Equal(categoryId, domainEvent.CategoryId);
    }

    [Fact]
    public void Update_WhenValuesAreUnchanged_ReturnsFalseWithoutDomainEvent()
    {
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Keyboard", "Mechanical keyboard", 99.9m, categoryId).Value;
        product.ClearDomainEvents();

        var result = product.Update(" Keyboard ", "Mechanical keyboard", 99.9m, categoryId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public void Update_WhenPriceChanges_RaisesPriceChangedEvent()
    {
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Keyboard", "Mechanical keyboard", 99.9m, categoryId).Value;
        product.ClearDomainEvents();

        var result = product.Update("Keyboard", "Mechanical keyboard", 109.9m, categoryId);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var domainEvent = Assert.IsType<ProductPriceChangedEvent>(Assert.Single(product.DomainEvents));
        Assert.Equal(product.ProductId, domainEvent.ProductId);
        Assert.Equal(99.9m, domainEvent.OldPrice);
        Assert.Equal(109.9m, domainEvent.NewPrice);
    }
}
