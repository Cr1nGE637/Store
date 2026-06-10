namespace Store.Catalog.Infrastructure.Services;

public sealed class ProductImageStorageOptions
{
    public string RootPath { get; init; } = "wwwroot/uploads/products";
    public string PublicBasePath { get; init; } = "/uploads/products";
    public long MaxFileSizeBytes { get; init; } = 5 * 1024 * 1024;
    public IReadOnlyCollection<string> AllowedContentTypes { get; init; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];
}
