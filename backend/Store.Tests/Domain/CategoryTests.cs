using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Enums;

namespace Store.Tests.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidElectronicsCode_CreatesCategory()
    {
        var result = Category.Create(" Smartphones ", "smartphone");

        Assert.True(result.IsSuccess);
        Assert.Equal("Smartphones", result.Value.CategoryName);
        Assert.Equal(ElectronicsCategoryCode.Smartphone, result.Value.CategoryCode);
    }

    [Fact]
    public void Create_WithInvalidElectronicsCode_ReturnsFailure()
    {
        var result = Category.Create("Furniture", "Furniture");

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid electronics category code", result.Error);
    }

    [Fact]
    public void Update_WithValidElectronicsCode_ChangesNameAndCode()
    {
        var category = Category.Create("Accessories", "Accessory").Value;

        var result = category.Update("Peripherals", "Peripheral");

        Assert.True(result.IsSuccess);
        Assert.Equal("Peripherals", category.CategoryName);
        Assert.Equal(ElectronicsCategoryCode.Peripheral, category.CategoryCode);
    }
}
