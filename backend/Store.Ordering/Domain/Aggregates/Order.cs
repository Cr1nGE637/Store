using CSharpFunctionalExtensions;
using Store.Ordering.Domain.Enums;
using Store.Ordering.Contracts.Events;
using Store.Ordering.Domain.ValueObjects;
using Store.SharedKernel;

namespace Store.Ordering.Domain.Aggregates;

public class Order : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid? SourceCheckoutId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string PaymentMethod { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public decimal? PaidAmount { get; private set; }
    public string? PaymentTransactionId { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private readonly List<OrderedProduct> _products = [];
    public IReadOnlyList<OrderedProduct> Products => _products.AsReadOnly();
    public decimal TotalAmount => _products.Sum(p => p.Price * p.Quantity);

    private Order(
        Guid orderId,
        Guid? sourceCheckoutId,
        Guid customerId,
        string customerEmail,
        string recipientName,
        string phone,
        string paymentMethod,
        OrderStatus status,
        DateTime createdAt)
    {
        OrderId = orderId;
        SourceCheckoutId = sourceCheckoutId;
        CustomerId = customerId;
        CustomerEmail = customerEmail;
        RecipientName = recipientName;
        Phone = phone;
        PaymentMethod = paymentMethod;
        Status = status;
        CreatedAt = createdAt;
    }

    public static Result<Order> Create(
        Guid customerId,
        string customerEmail,
        string recipientName,
        string phone,
        string paymentMethod,
        IReadOnlyList<OrderedProduct> products,
        Guid? sourceCheckoutId = null)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Order>("CustomerId is required");
        if (string.IsNullOrWhiteSpace(customerEmail))
            return Result.Failure<Order>("Customer email is required");
        var checkoutDetailsResult = CheckoutDetails.Create(
            recipientName,
            phone,
            paymentMethod);
        if (checkoutDetailsResult.IsFailure)
            return Result.Failure<Order>(checkoutDetailsResult.Error);
        if (sourceCheckoutId == Guid.Empty)
            return Result.Failure<Order>("SourceCheckoutId cannot be empty");
        if (products.Count == 0)
            return Result.Failure<Order>("Order must contain at least one product");
        if (products.GroupBy(p => p.ProductId).Any(g => g.Count() > 1))
            return Result.Failure<Order>("Order cannot contain duplicate products");

        var order = new Order(
            Guid.NewGuid(),
            sourceCheckoutId,
            customerId,
            customerEmail.Trim(),
            checkoutDetailsResult.Value.RecipientName.Value,
            checkoutDetailsResult.Value.Phone.Value,
            checkoutDetailsResult.Value.PaymentMethod.Value,
            OrderStatus.AwaitingStock,
            DateTime.UtcNow);
        order._products.AddRange(products);
        order.RaiseDomainEvent(new OrderStockReservationRequestedEvent(order.OrderId, order.CustomerId, order.CustomerEmail, order.MapToItems()));
        return Result.Success(order);
    }

    internal static Order Reconstitute(
        Guid orderId, Guid? sourceCheckoutId, Guid customerId, string customerEmail,
        string recipientName, string phone, string paymentMethod, OrderStatus status,
        DateTime createdAt, DateTime? paidAt, DateTime? cancelledAt, DateTime? rejectedAt,
        decimal? paidAmount, string? paymentTransactionId, string? rejectionReason) =>
        new(orderId, sourceCheckoutId, customerId, customerEmail, recipientName, phone, paymentMethod, status, createdAt)
        {
            PaidAt = paidAt,
            PaidAmount = paidAmount,
            PaymentTransactionId = paymentTransactionId,
            CancelledAt = cancelledAt,
            RejectedAt = rejectedAt,
            RejectionReason = rejectionReason
        };

    internal void LoadProducts(IEnumerable<OrderedProduct> products) => _products.AddRange(products);

    public Result<bool> MarkAsPaid(decimal paidAmount, string transactionId)
    {
        if (Status == OrderStatus.Paid)
            return Result.Success(false);
        if (Status == OrderStatus.AwaitingStock)
            return Result.Failure<bool>("Order is awaiting stock reservation");
        if (Status == OrderStatus.Rejected)
            return Result.Failure<bool>("Rejected orders cannot be paid");
        if (Status == OrderStatus.Cancelled)
            return Result.Failure<bool>("Отмененный заказ нельзя оплатить");
        if (paidAmount <= 0)
            return Result.Failure<bool>("Paid amount must be positive");
        if (paidAmount != TotalAmount)
            return Result.Failure<bool>("Paid amount must match order total");
        if (string.IsNullOrWhiteSpace(transactionId))
            return Result.Failure<bool>("Payment transaction id is required");

        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
        PaidAmount = paidAmount;
        PaymentTransactionId = transactionId.Trim();
        RaiseDomainEvent(new OrderPaidEvent(OrderId, CustomerId, CustomerEmail, MapToItems()));
        return Result.Success(true);
    }

    public Result<bool> Cancel()
    {
        if (Status == OrderStatus.Paid)
            return Result.Failure<bool>("Paid orders cannot be cancelled");
        if (Status == OrderStatus.Rejected)
            return Result.Success(false);
        if (Status == OrderStatus.Cancelled)
            return Result.Success(false);

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderCancelledEvent(OrderId, CustomerId, CustomerEmail, MapToItems(), SourceCheckoutId));
        return Result.Success(true);
    }

    public Result<bool> ConfirmStockReserved()
    {
        if (Status == OrderStatus.Unpaid)
            return Result.Success(false);
        if (Status != OrderStatus.AwaitingStock)
            return Result.Failure<bool>($"Cannot confirm stock reservation for order in {Status} status");

        Status = OrderStatus.Unpaid;
        RaiseDomainEvent(new OrderCreatedEvent(OrderId, SourceCheckoutId, CustomerId, CustomerEmail, MapToItems()));
        return Result.Success(true);
    }

    public Result<bool> RejectStockReservation(string reason)
    {
        if (Status == OrderStatus.Rejected)
            return Result.Success(false);
        if (Status != OrderStatus.AwaitingStock)
            return Result.Failure<bool>($"Cannot reject stock reservation for order in {Status} status");

        Status = OrderStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Stock reservation rejected" : reason;
        RaiseDomainEvent(new OrderRejectedEvent(OrderId, SourceCheckoutId, CustomerId, CustomerEmail, RejectionReason, MapToItems()));
        return Result.Success(true);
    }

    private IReadOnlyList<OrderItem> MapToItems() =>
        _products.Select(p => new OrderItem(p.ProductId, p.ProductName, p.Price, p.Quantity))
                 .ToList()
                 .AsReadOnly();
}
