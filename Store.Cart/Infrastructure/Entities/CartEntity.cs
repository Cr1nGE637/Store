namespace Store.Carts.Infrastructure.Entities;

public class CartEntity
{
    public Guid CartId { get; set; }
    public Guid CustomerId { get; set; }

    public List<CartItemEntity> Items { get; set; } = [];
}
