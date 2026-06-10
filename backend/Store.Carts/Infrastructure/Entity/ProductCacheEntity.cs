namespace Store.Carts.Infrastructure.Entity;

public class ProductCacheEntity
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public string? MainImageAltText { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<CartItemEntity> CartItems { get; set; } = [];
}
