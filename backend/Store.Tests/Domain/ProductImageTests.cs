using Store.Catalog.Domain.Entities;

namespace Store.Tests.Domain;

public class ProductImageTests
{
    [Fact]
    public void Create_WhenMetadataIsValid_ReturnsImage()
    {
        var result = ProductImage.Create(
            Guid.NewGuid(),
            "/uploads/products/product/image.jpg",
            "product/image.jpg",
            "image.jpg",
            "image/jpeg",
            12345,
            "Gaming laptop front view",
            isMain: true,
            displayOrder: 0);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsMain);
        Assert.Equal("Gaming laptop front view", result.Value.AltText);
        Assert.Equal(0, result.Value.DisplayOrder);
    }

    [Theory]
    [InlineData(0, "Image file cannot be empty")]
    [InlineData(-1, "Image file cannot be empty")]
    public void Create_WhenSizeIsInvalid_ReturnsFailure(long sizeBytes, string expectedError)
    {
        var result = ProductImage.Create(
            Guid.NewGuid(),
            "/uploads/products/product/image.jpg",
            "product/image.jpg",
            "image.jpg",
            "image/jpeg",
            sizeBytes,
            null,
            isMain: true,
            displayOrder: 0);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public void Create_WhenDisplayOrderIsNegative_ReturnsFailure()
    {
        var result = ProductImage.Create(
            Guid.NewGuid(),
            "/uploads/products/product/image.jpg",
            "product/image.jpg",
            "image.jpg",
            "image/jpeg",
            12345,
            null,
            isMain: true,
            displayOrder: -1);

        Assert.True(result.IsFailure);
        Assert.Equal("Image display order cannot be negative", result.Error);
    }
}
