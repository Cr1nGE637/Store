using CSharpFunctionalExtensions;

namespace Store.Catalog.Domain.Entities;

public sealed class ProductImage
{
    public Guid ProductImageId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Url { get; private set; }
    public string StoragePath { get; private set; }
    public string OriginalFileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public string AltText { get; private set; }
    public bool IsMain { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ProductImage(
        Guid productImageId,
        Guid productId,
        string url,
        string storagePath,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string altText,
        bool isMain,
        int displayOrder,
        DateTime createdAtUtc)
    {
        ProductImageId = productImageId;
        ProductId = productId;
        Url = url;
        StoragePath = storagePath;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        AltText = altText;
        IsMain = isMain;
        DisplayOrder = displayOrder;
        CreatedAtUtc = createdAtUtc;
    }

    public static Result<ProductImage> Create(
        Guid productId,
        string url,
        string storagePath,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string? altText,
        bool isMain,
        int displayOrder)
    {
        if (productId == Guid.Empty)
            return Result.Failure<ProductImage>("Product is required");

        if (string.IsNullOrWhiteSpace(url))
            return Result.Failure<ProductImage>("Image URL is required");

        if (string.IsNullOrWhiteSpace(storagePath))
            return Result.Failure<ProductImage>("Image storage path is required");

        if (string.IsNullOrWhiteSpace(originalFileName))
            return Result.Failure<ProductImage>("Original file name is required");

        if (string.IsNullOrWhiteSpace(contentType))
            return Result.Failure<ProductImage>("Image content type is required");

        if (sizeBytes <= 0)
            return Result.Failure<ProductImage>("Image file cannot be empty");

        if (displayOrder < 0)
            return Result.Failure<ProductImage>("Image display order cannot be negative");

        var normalizedUrl = url.Trim();
        var normalizedStoragePath = storagePath.Trim();
        var normalizedOriginalFileName = originalFileName.Trim();
        var normalizedContentType = contentType.Trim();
        var normalizedAltText = (altText ?? string.Empty).Trim();

        if (normalizedUrl.Length > 500)
            return Result.Failure<ProductImage>("Image URL cannot exceed 500 characters");

        if (normalizedStoragePath.Length > 500)
            return Result.Failure<ProductImage>("Image storage path cannot exceed 500 characters");

        if (normalizedOriginalFileName.Length > 255)
            return Result.Failure<ProductImage>("Original file name cannot exceed 255 characters");

        if (normalizedContentType.Length > 100)
            return Result.Failure<ProductImage>("Image content type cannot exceed 100 characters");

        if (normalizedAltText.Length > 200)
            return Result.Failure<ProductImage>("Image alt text cannot exceed 200 characters");

        return Result.Success(new ProductImage(
            Guid.NewGuid(),
            productId,
            normalizedUrl,
            normalizedStoragePath,
            normalizedOriginalFileName,
            normalizedContentType,
            sizeBytes,
            normalizedAltText,
            isMain,
            displayOrder,
            DateTime.UtcNow));
    }

    internal static ProductImage Reconstitute(
        Guid productImageId,
        Guid productId,
        string url,
        string storagePath,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string altText,
        bool isMain,
        int displayOrder,
        DateTime createdAtUtc) =>
        new(
            productImageId,
            productId,
            url,
            storagePath,
            originalFileName,
            contentType,
            sizeBytes,
            altText,
            isMain,
            displayOrder,
            createdAtUtc);
}
