namespace Store.Catalog.Infrastructure.Entity;

public class ProductSpecificationEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public ProductEntity Product { get; set; } = null!;
}
