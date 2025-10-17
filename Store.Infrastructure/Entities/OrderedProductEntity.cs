using Store.Domain.Aggregates;
using Store.Domain.ValueObjects;

namespace Store.Infrastructure.Entities;

public class OrderedProductEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public string? ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    
    public OrderEntity? Order { get; set; }
    
}
