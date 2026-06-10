using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Enums;

namespace Store.Tests.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidElectronicsCode_CreatesCategory()
    {
        var result = Category.Create(" Smartphones ", "smartphones");

        Assert.True(result.IsSuccess);
        Assert.Equal("Smartphones", result.Value.CategoryName);
        Assert.Equal(ElectronicsCategoryCode.Smartphones, result.Value.CategoryCode);
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
        var category = Category.Create("Accessories", "Accessories").Value;

        var result = category.Update("Peripherals", "Peripherals");

        Assert.True(result.IsSuccess);
        Assert.Equal("Peripherals", category.CategoryName);
        Assert.Equal(ElectronicsCategoryCode.Peripherals, category.CategoryCode);
    }

    [Fact]
    public void Create_WithNewConsumerElectronicsCode_CreatesCategory()
    {
        var result = Category.Create("USB-C chargers", "chargers");

        Assert.True(result.IsSuccess);
        Assert.Equal("USB-C chargers", result.Value.CategoryName);
        Assert.Equal(ElectronicsCategoryCode.Chargers, result.Value.CategoryCode);
    }
}
