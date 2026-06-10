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

    [Fact]
    public void Create_WhenSpecificationUsesAlias_StoresCanonicalSpecificationName()
    {
        var result = Product.Create(
            "SSD-001",
            "NVMe SSD",
            "Storage",
            159.9m,
            "Kingston",
            "KC3000",
            60,
            Guid.NewGuid(),
            new Dictionary<string, string>
            {
                [" form_factor "] = "M.2 2280",
                ["PowerConsumptionWatts"] = "8"
            });

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Specifications, s => s.Name == "formFactor" && s.Value == "M.2 2280");
        Assert.Contains(result.Value.Specifications, s => s.Name == "powerConsumptionWatts" && s.Value == "8");
    }

    [Fact]
    public void Create_WhenNumericSpecificationUsesUnit_StoresValue()
    {
        var result = Product.Create(
            "MON-001",
            "Gaming Monitor",
            "Monitor",
            349.9m,
            "LG",
            "27GP850-B",
            24,
            Guid.NewGuid(),
            new Dictionary<string, string>
            {
                ["screenSize"] = "27",
                ["refreshRate"] = "165Hz",
                ["resolution"] = "2560x1440"
            });

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Specifications, s => s.Name == "refreshRate" && s.Value == "165Hz");
    }

    [Fact]
    public void Create_WhenNumericSpecificationValueIsInvalid_ReturnsFailure()
    {
        var result = Product.Create(
            "CHG-001",
            "USB-C Charger",
            "Charger",
            49.9m,
            "Baseus",
            "GaN5",
            12,
            Guid.NewGuid(),
            new Dictionary<string, string>
            {
                ["powerWatts"] = "fast",
                ["connectorType"] = "USB-C"
            });

        Assert.True(result.IsFailure);
        Assert.Contains("powerWatts", result.Error);
    }

    [Fact]
    public void Create_WhenResolutionSpecificationValueIsInvalid_ReturnsFailure()
    {
        var result = Product.Create(
            "TV-001",
            "Smart TV",
            "TV",
            799.9m,
            "Samsung",
            "Q80D",
            24,
            Guid.NewGuid(),
            new Dictionary<string, string>
            {
                ["resolution"] = "very sharp"
            });

        Assert.True(result.IsFailure);
        Assert.Contains("resolution", result.Error);
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
