namespace Store.Carts.Infrastructure.Entity;

public class CheckoutOrderEntity
{
    public Guid CheckoutId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
