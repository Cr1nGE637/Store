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

        var result = Order.Create(customerId, "customer@example.com", [product]);

        Assert.True(result.IsSuccess);
        var order = result.Value;
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal("customer@example.com", order.CustomerEmail);
        Assert.Equal(OrderStatus.AwaitingStock, order.Status);
        Assert.Single(order.Products);

        var domainEvent = Assert.IsType<OrderStockReservationRequestedEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.OrderId, domainEvent.OrderId);
        Assert.Equal(customerId, domainEvent.CustomerId);
        Assert.Equal("customer@example.com", domainEvent.CustomerEmail);
        Assert.Single(domainEvent.Items);
    }

    [Fact]
    public void ConfirmStockReserved_WhenAwaitingStock_MarksUnpaidAndRaisesCreatedEvent()
    {
        var order = Order.Create(Guid.NewGuid(), "customer@example.com", [CreateProduct()]).Value;
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
        var order = Order.Create(Guid.NewGuid(), "customer@example.com", [CreateProduct()]).Value;
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

        var firstResult = order.MarkAsPaid();
        var secondResult = order.MarkAsPaid();

        Assert.True(firstResult.IsSuccess);
        Assert.True(firstResult.Value);
        Assert.True(secondResult.IsSuccess);
        Assert.False(secondResult.Value);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.NotNull(order.PaidAt);
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
        order.MarkAsPaid();
        order.ClearDomainEvents();

        var result = order.Cancel();

        Assert.True(result.IsFailure);
        Assert.Equal("Paid orders cannot be cancelled", result.Error);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Empty(order.DomainEvents);
    }

    private static Order CreateOrder() =>
        CreateConfirmedOrder();

    private static Order CreateConfirmedOrder()
    {
        var order = Order.Create(Guid.NewGuid(), "customer@example.com", [CreateProduct()]).Value;
        order.ConfirmStockReserved();
        return order;
    }

    private static OrderedProduct CreateProduct() =>
        OrderedProduct.Create(Guid.NewGuid(), "Keyboard", 99.9m, 2).Value;
}
