namespace Store.Catalog.Infrastructure.Entity;

public sealed class ProductImageEntity
{
    public Guid ProductImageId { get; set; }
    public Guid ProductId { get; set; }
    public ProductEntity Product { get; set; } = null!;
    public string Url { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string AltText { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
