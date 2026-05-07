namespace Store.Carts.Infrastructure.Entity;

public class CartEntity
{
    public Guid CartId { get; set; }
    public Guid CustomerId { get; set; }
    public bool IsCheckoutPending { get; set; }

    public List<CartItemEntity> Items { get; set; } = [];
}
