namespace Store.Catalog.Infrastructure.Entity;

public class ProductAvailabilityEntity
{
    public Guid ProductId { get; set; }
    public int AvailableQuantity { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}
