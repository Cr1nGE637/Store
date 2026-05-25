using Store.Ordering.Contracts.Events;
using Store.Ordering.Domain.Aggregates;
using Store.Ordering.Domain.Enums;
using Store.Ordering.Domain.ValueObjects;

namespace Store.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_WithProducts_CreatesAwaitingStockOrderAndRaisesReservationRequest()
    {
        var customerId = Guid.NewGuid();
        var product = CreateProduct();

        var result = CreateOrder(customerId, [product]);

        Assert.True(result.IsSuccess);
        var order = result.Value;
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal("customer@example.com", order.CustomerEmail);
        Assert.Equal("Ivan Petrov", order.RecipientName);
        Assert.Equal("+79990000000", order.Phone);
        Assert.Equal("Card", order.PaymentMethod);
        Assert.Equal(199.8m, order.TotalAmount);
        Assert.Equal(OrderStatus.AwaitingStock, order.Status);
        Assert.Single(order.Products);

        var domainEvent = Assert.IsType<OrderStockReservationRequestedEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.OrderId, domainEvent.OrderId);
        Assert.Equal(customerId, domainEvent.CustomerId);
        Assert.Equal("customer@example.com", domainEvent.CustomerEmail);
        Assert.Single(domainEvent.Items);
    }

    [Fact]
    public void Create_WithMultipleProducts_CalculatesTotalAmount()
    {
        var result = CreateOrder(
            Guid.NewGuid(),
            [
                OrderedProduct.Create(Guid.NewGuid(), "Keyboard", 99.9m, 2).Value,
                OrderedProduct.Create(Guid.NewGuid(), "Mouse", 49.5m, 1).Value
            ]);

        Assert.True(result.IsSuccess);
        Assert.Equal(249.3m, result.Value.TotalAmount);
    }

    [Fact]
    public void Create_WithoutProducts_Fails()
    {
        var result = CreateOrder(Guid.NewGuid(), []);

        Assert.True(result.IsFailure);
        Assert.Equal("Order must contain at least one product", result.Error);
    }

    [Fact]
    public void ConfirmStockReserved_WhenAwaitingStock_MarksUnpaidAndRaisesCreatedEvent()
    {
        var order = CreateAwaitingOrder();
        order.ClearDomainEvents();

        var result = order.ConfirmStockReserved();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(OrderStatus.Unpaid, order.Status);
        Assert.IsType<OrderCreatedEvent>(Assert.Single(order.DomainEvents));
    }

    [Fact]
    public void RejectStockReservation_WhenAwaitingStock_MarksRejectedAndRaisesRejectedEvent()
    {
        var order = CreateAwaitingOrder();
        order.ClearDomainEvents();

        var result = order.RejectStockReservation("Insufficient stock");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(OrderStatus.Rejected, order.Status);
        Assert.Equal("Insufficient stock", order.RejectionReason);
        Assert.NotNull(order.RejectedAt);
        Assert.IsType<OrderRejectedEvent>(Assert.Single(order.DomainEvents));
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_IsSuccessfulNoOpWithoutNewEvent()
    {
        var order = CreateOrder();
        order.ClearDomainEvents();

        var firstResult = Pay(order);
        var secondResult = Pay(order);

        Assert.True(firstResult.IsSuccess);
        Assert.True(firstResult.Value);
        Assert.True(secondResult.IsSuccess);
        Assert.False(secondResult.Value);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.NotNull(order.PaidAt);
        Assert.Equal(order.TotalAmount, order.PaidAmount);
        Assert.Equal("test-transaction", order.PaymentTransactionId);
        Assert.IsType<OrderPaidEvent>(Assert.Single(order.DomainEvents));
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_IsSuccessfulNoOpWithoutNewEvent()
    {
        var order = CreateOrder();
        order.ClearDomainEvents();

        var firstResult = order.Cancel();
        var secondResult = order.Cancel();

        Assert.True(firstResult.IsSuccess);
        Assert.True(firstResult.Value);
        Assert.True(secondResult.IsSuccess);
        Assert.False(secondResult.Value);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.NotNull(order.CancelledAt);
        Assert.IsType<OrderCancelledEvent>(Assert.Single(order.DomainEvents));
    }

    [Fact]
    public void Cancel_WhenOrderIsPaid_FailsWithoutChangingStatus()
    {
        var order = CreateOrder();
        Pay(order);
        order.ClearDomainEvents();

        var result = order.Cancel();

        Assert.True(result.IsFailure);
        Assert.Equal("Paid orders cannot be cancelled", result.Error);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Empty(order.DomainEvents);
    }

    private static Order CreateOrder() =>
        CreateConfirmedOrder();

    private static Order CreateAwaitingOrder() =>
        CreateOrder(Guid.NewGuid(), [CreateProduct()]).Value;

    private static CSharpFunctionalExtensions.Result<Order> CreateOrder(Guid customerId, IReadOnlyList<OrderedProduct> products) =>
        Order.Create(
            customerId,
            "customer@example.com",
            "Ivan Petrov",
            "+79990000000",
            "Card",
            products);

    private static Order CreateConfirmedOrder()
    {
        var order = CreateAwaitingOrder();
        order.ConfirmStockReserved();
        return order;
    }

    private static CSharpFunctionalExtensions.Result<bool> Pay(Order order) =>
        order.MarkAsPaid(order.TotalAmount, "test-transaction");

    private static OrderedProduct CreateProduct() =>
        OrderedProduct.Create(Guid.NewGuid(), "Keyboard", 99.9m, 2).Value;
}
