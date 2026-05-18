using Store.Catalog.Contracts.Events;
using Store.Catalog.Domain.Entities;

namespace Store.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_TrimsNameAndRaisesCreatedEvent()
    {
        var categoryId = Guid.NewGuid();

        var result = Product.Create(
            " kb-001 ",
            "  Keyboard  ",
            "Mechanical keyboard",
            99.9m,
            "Keychron",
            "K8 Pro",
            24,
            categoryId,
            new Dictionary<string, string> { ["Switches"] = "Brown" });

        Assert.True(result.IsSuccess);
        var product = result.Value;
        Assert.Equal("KB-001", product.Sku.Value);
        Assert.Equal("Keyboard", product.ProductName);
        Assert.Equal("Keychron", product.Brand);
        Assert.Equal("K8 Pro", product.Model);
        Assert.Equal(24, product.WarrantyMonths);
        Assert.Contains(product.Specifications, s => s.Name == "Switches" && s.Value == "Brown");

        var domainEvent = Assert.IsType<ProductCreatedEvent>(Assert.Single(product.DomainEvents));
        Assert.Equal(product.ProductId, domainEvent.ProductId);
        Assert.Equal("KB-001", domainEvent.Sku);
        Assert.Equal("Keyboard", domainEvent.ProductName);
        Assert.Equal("Keychron", domainEvent.Brand);
        Assert.Equal("K8 Pro", domainEvent.Model);
        Assert.Equal(24, domainEvent.WarrantyMonths);
        Assert.Equal(99.9m, domainEvent.Price);
        Assert.Equal(categoryId, domainEvent.CategoryId);
    }

    [Fact]
    public void Update_WhenValuesAreUnchanged_ReturnsFalseWithoutDomainEvent()
    {
        var categoryId = Guid.NewGuid();
        var specifications = new Dictionary<string, string> { ["Switches"] = "Brown" };
        var product = Product.Create("KB-001", "Keyboard", "Mechanical keyboard", 99.9m, "Keychron", "K8 Pro", 24, categoryId, specifications).Value;
        product.ClearDomainEvents();

        var result = product.Update(" kb-001 ", " Keyboard ", "Mechanical keyboard", 99.9m, "Keychron", "K8 Pro", 24, categoryId, specifications);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public void Update_WhenPriceChanges_RaisesPriceChangedEvent()
    {
        var categoryId = Guid.NewGuid();
        var product = Product.Create("KB-001", "Keyboard", "Mechanical keyboard", 99.9m, "Keychron", "K8 Pro", 24, categoryId, null).Value;
        product.ClearDomainEvents();

        var result = product.Update("KB-001", "Keyboard", "Mechanical keyboard", 109.9m, "Keychron", "K8 Pro", 24, categoryId, null);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var domainEvent = Assert.IsType<ProductPriceChangedEvent>(Assert.Single(product.DomainEvents));
        Assert.Equal(product.ProductId, domainEvent.ProductId);
        Assert.Equal(99.9m, domainEvent.OldPrice);
        Assert.Equal(109.9m, domainEvent.NewPrice);
    }

    [Theory]
    [InlineData("", "Keyboard", "Keychron", "K8 Pro", 24, "SKU is required")]
    [InlineData("KB-001", "", "Keychron", "K8 Pro", 24, "Product name is required")]
    [InlineData("KB-001", "Keyboard", "", "K8 Pro", 24, "Brand is required")]
    [InlineData("KB-001", "Keyboard", "Keychron", "", 24, "Model is required")]
    [InlineData("KB-001", "Keyboard", "Keychron", "K8 Pro", -1, "Warranty cannot be negative")]
    public void Create_WhenElectronicsDetailsAreInvalid_ReturnsFailure(
        string sku,
        string name,
        string brand,
        string model,
        int warrantyMonths,
        string expectedError)
    {
        var result = Product.Create(
            sku,
            name,
            "Mechanical keyboard",
            99.9m,
            brand,
            model,
            warrantyMonths,
            Guid.NewGuid(),
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }
}
