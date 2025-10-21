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
    
    private Order(Guid orderId, Guid customerId, DateTime orderDate, OrderStatus status, decimal totalPrice)
    {
        OrderId = orderId;
        CustomerId = customerId;
        OrderDate = orderDate;
        Status = status;
        TotalPrice = totalPrice;
    }

    public static Result<Order> Restore(
        Guid orderId,
        Guid customerId,
        DateTime orderDate,
        OrderStatus status,
        decimal totalPrice,
        List<OrderedProduct> products)
    {
        var order = new Order(orderId, customerId, orderDate, status, totalPrice);
        order._products.AddRange(products);
        return Result.Success(order);
    }

    public static Result<Order> Create(Guid customerId)
    {
        var order = new Order(Guid.NewGuid(), customerId, DateTime.Now, OrderStatus.Unpaid, totalPrice: 0);
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

    public Result UpdateProduct(Guid productId, int quantity, string productName, decimal productPrice)
    {
        var product = _products.FirstOrDefault(p => p.ProductId == productId);
        if (product == null)
        {
            return Result.Failure($"Product with id {productId} not found");
        }
        var createResult = OrderedProduct.Create(productId, quantity, productName, productPrice);
        if (createResult.IsFailure)
        {
            return Result.Failure("Failed to create product");
        }
        var updatedProduct = createResult.Value;
        _products.Remove(product);
        _products.Add(updatedProduct);
        CalculateTotalPrice();
        return Result.Success();
}
    public void CalculateTotalPrice()
    {
        TotalPrice = _products.Sum(p => p.Price);
    }
}