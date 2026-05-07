using CSharpFunctionalExtensions;
using Store.Ordering.Domain.Enums;
using Store.Ordering.Contracts.Events;
using Store.Ordering.Domain.ValueObjects;
using Store.SharedKernel;

namespace Store.Ordering.Domain.Aggregates;

public class Order : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private readonly List<OrderedProduct> _products = [];
    public IReadOnlyList<OrderedProduct> Products => _products.AsReadOnly();

    private Order(Guid orderId, Guid customerId, string customerEmail, OrderStatus status, DateTime createdAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        CustomerEmail = customerEmail;
        Status = status;
        CreatedAt = createdAt;
    }

    public static Result<Order> Create(Guid customerId, string customerEmail, IReadOnlyList<OrderedProduct> products)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Order>("CustomerId is required");
        if (products.Count == 0)
            return Result.Failure<Order>("Order must contain at least one product");
        if (products.GroupBy(p => p.ProductId).Any(g => g.Count() > 1))
            return Result.Failure<Order>("Order cannot contain duplicate products");

        var order = new Order(Guid.NewGuid(), customerId, customerEmail, OrderStatus.Unpaid, DateTime.UtcNow);
        order._products.AddRange(products);
        order.RaiseDomainEvent(new OrderCreatedEvent(order.OrderId, order.CustomerId, order.CustomerEmail, order.MapToItems()));
        return Result.Success(order);
    }

    internal static Order Reconstitute(
        Guid orderId, Guid customerId, string customerEmail, OrderStatus status,
        DateTime createdAt, DateTime? paidAt, DateTime? cancelledAt) =>
        new(orderId, customerId, customerEmail, status, createdAt)
        {
            PaidAt = paidAt,
            CancelledAt = cancelledAt
        };

    internal void LoadProducts(IEnumerable<OrderedProduct> products) => _products.AddRange(products);

    public Result<bool> MarkAsPaid()
    {
        if (Status == OrderStatus.Paid)
            return Result.Success(false);
        if (Status == OrderStatus.Cancelled)
            return Result.Failure<bool>("Cancelled orders cannot be paid");

        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderPaidEvent(OrderId, CustomerId, CustomerEmail, MapToItems()));
        return Result.Success(true);
    }

    public Result<bool> Cancel()
    {
        if (Status == OrderStatus.Paid)
            return Result.Failure<bool>("Paid orders cannot be cancelled");
        if (Status == OrderStatus.Cancelled)
            return Result.Success(false);

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderCancelledEvent(OrderId, CustomerId, CustomerEmail, MapToItems()));
        return Result.Success(true);
    }

    private IReadOnlyList<OrderItem> MapToItems() =>
        _products.Select(p => new OrderItem(p.ProductId, p.ProductName, p.Price, p.Quantity))
                 .ToList()
                 .AsReadOnly();
}
