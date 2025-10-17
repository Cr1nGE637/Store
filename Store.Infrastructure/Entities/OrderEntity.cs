using Store.Domain.Entities;
using Store.Domain.Enums;

namespace Store.Infrastructure.Entities;

public class OrderEntity
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; }
    public CustomerEntity? Customer { get; set; }
    public List<OrderedProductEntity>? OrderedProducts { get; set; }
}