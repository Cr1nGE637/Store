namespace Store.Catalog.Infrastructure.Entity;

public class ProductEntity
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int WarrantyMonths { get; set; }
    public Guid CategoryId { get; set; }
    public ProductAvailabilityEntity? Availability { get; set; }
    public List<ProductSpecificationEntity> Specifications { get; set; } = [];
    public List<ProductImageEntity> Images { get; set; } = [];
}
