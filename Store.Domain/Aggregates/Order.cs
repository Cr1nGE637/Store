using CSharpFunctionalExtensions;
using Store.Domain.Entities;
using Store.Domain.Enums;
using Store.Domain.ValueObjects;

namespace Store.Domain.Aggregates;

public class Order
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public decimal TotalPrice { get; private set; }
    public OrderStatus Status { get; private set; }
    
    private readonly List<OrderedProduct> _products = [];
    public IReadOnlyList<OrderedProduct> Products => _products.AsReadOnly();

    private Order(Guid orderId, Guid customerId, DateTime orderDate)
    {
        OrderId = orderId;
        CustomerId = customerId;
        OrderDate = orderDate;
    }

    public static Result<Order> Create(Guid customerId)
    {
        var order = new Order(Guid.NewGuid(), customerId, DateTime.Now);
        return Result.Success(order);
    }
    public void AddProduct(OrderedProduct product)
    {
         _products.Add(product);
    }
    public void RemoveProduct(OrderedProduct product)
    {
        _products.Remove(product);
    }
    public void RemoveAllProducts()
    {
        _products.Clear();
    }
    public void CalculateTotalPrice()
    {
        TotalPrice = _products.Sum(p => p.Price);
    }
}