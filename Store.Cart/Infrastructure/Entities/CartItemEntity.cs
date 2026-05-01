namespace Store.Carts.Infrastructure.Entities;

public class CartItemEntity
{
    public Guid CartItemId { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public CartEntity Cart { get; set; } = null!;
}
