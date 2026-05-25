namespace Store.Carts.Infrastructure.Entity;

public class CartEntity
{
    public Guid CartId { get; set; }
    public Guid CustomerId { get; set; }
    public bool IsCheckoutPending { get; set; }
    public Guid? PendingCheckoutId { get; set; }
    public DateTimeOffset? CheckoutPendingSince { get; set; }

    public List<CartItemEntity> Items { get; set; } = [];
}
