using CSharpFunctionalExtensions;
using Store.Ordering.Domain.Enums;
using Store.Ordering.Domain.Events;
using Store.Ordering.Domain.ValueObjects;
using Store.SharedKernel;

namespace Store.Ordering.Domain.Aggregates;

public class Order : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private readonly List<OrderedProduct> _products = [];
    public IReadOnlyList<OrderedProduct> Products => _products.AsReadOnly();

    private Order(Guid orderId, Guid customerId, OrderStatus status, DateTime createdAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Status = status;
        CreatedAt = createdAt;
    }

    public static Result<Order> Create(Guid customerId, IReadOnlyList<OrderedProduct> products)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Order>("CustomerId is required");
        if (products.Count == 0)
            return Result.Failure<Order>("Order must contain at least one product");
        if (products.GroupBy(p => p.ProductId).Any(g => g.Count() > 1))
            return Result.Failure<Order>("Order cannot contain duplicate products");

        var order = new Order(Guid.NewGuid(), customerId, OrderStatus.Unpaid, DateTime.UtcNow);
        order._products.AddRange(products);
        order.RaiseDomainEvent(new OrderCreatedEvent(order.OrderId, order.CustomerId, order.MapToItems()));
        return Result.Success(order);
    }

    internal static Order Reconstitute(
        Guid orderId, Guid customerId, OrderStatus status,
        DateTime createdAt, DateTime? paidAt, DateTime? cancelledAt) =>
        new(orderId, customerId, status, createdAt)
        {
            PaidAt = paidAt,
            CancelledAt = cancelledAt
        };

    internal void LoadProducts(IEnumerable<OrderedProduct> products) => _products.AddRange(products);

    public Result MarkAsPaid()
    {
        if (Status != OrderStatus.Unpaid)
            return Result.Failure("Only unpaid orders can be paid");

        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderPaidEvent(OrderId, CustomerId));
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == OrderStatus.Paid)
            return Result.Failure("Paid orders cannot be cancelled");
        if (Status == OrderStatus.Cancelled)
            return Result.Failure("Order is already cancelled");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderCancelledEvent(OrderId, CustomerId, MapToItems()));
        return Result.Success();
    }

    private IReadOnlyList<OrderItem> MapToItems() =>
        _products.Select(p => new OrderItem(p.ProductId, p.ProductName, p.Price, p.Quantity))
                 .ToList()
                 .AsReadOnly();
}
