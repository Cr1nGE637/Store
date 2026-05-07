using Store.Ordering.Domain.Enums;

namespace Store.Ordering.Infrastructure.Entity;

public class OrderEntity
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<OrderedProductEntity> Products { get; set; } = [];
}
