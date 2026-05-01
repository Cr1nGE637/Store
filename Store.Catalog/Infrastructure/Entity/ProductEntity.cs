namespace Store.Catalog.Infrastructure.Entity;

public class ProductEntity
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
}