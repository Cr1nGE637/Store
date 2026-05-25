using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Store.Carts.Contracts.Events;
using Store.EventOutbox.Application.Interfaces;
using Store.Inventory.Application.EventHandlers;
using Store.Inventory.Application.Interfaces;
using Store.Inventory.Contracts.Events;
using Store.Inventory.Domain.Aggregates;
using Store.Inventory.Domain.Interfaces;
using Store.Notifications.Application.EventHandlers;
using Store.Notifications.Application.Interfaces;
using Store.Ordering.Application.CQRS.Command;
using Store.Ordering.Application.EventHandlers;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Contracts.Events;
using Store.Ordering.Domain.Aggregates;
using Store.Ordering.Domain.Enums;
using Store.Ordering.Domain.Interfaces;
using Store.SharedKernel.Events;

namespace Store.Tests.Application;

public class OnlinePurchaseFlowTests
{
    [Fact]
    public async Task Customer_can_checkout_and_pay_order_flow()
    {
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var stockItem = StockItem.Create(productId, 2).Value;
        stockItem.ClearDomainEvents();

        var orderRepository = new FakeOrderRepository();
        var stockRepository = new FakeStockItemRepository(stockItem);
        var inbox = new FakeDomainEventInbox();
        var orderingOutbox = new FakeOrderingOutbox();
        var inventoryOutbox = new FakeInventoryOutbox();
        var notificationOutbox = new FakeNotificationOutbox();

        var checkoutEvent = new CartCheckedOutEvent(
            Guid.NewGuid(),
            customerId,
            "customer@example.com",
            "Ivan Petrov",
            "+79990000000",
            "Tomsk, Lenina 1",
            "Courier",
            "Card",
            [new CartCheckedOutItem(productId, "Apple iPhone 15", 79990m, 1)]);

        var checkoutHandler = new CartCheckedOutEventHandler(
            orderRepository,
            new FakeOrderingUnitOfWork(),
            orderingOutbox,
            inbox,
            NullLogger<CartCheckedOutEventHandler>.Instance);

        await checkoutHandler.Handle(checkoutEvent, CancellationToken.None);

        var order = Assert.Single(orderRepository.Orders);
        Assert.Equal(OrderStatus.AwaitingStock, order.Status);
        var reservationRequest = Assert.IsType<OrderStockReservationRequestedEvent>(Assert.Single(orderingOutbox.Events));

        var reservationHandler = new OrderStockReservationRequestedEventHandler(
            stockRepository,
            new FakeInventoryUnitOfWork(),
            inventoryOutbox,
            inbox,
            NullLogger<OrderStockReservationRequestedEventHandler>.Instance);

        await reservationHandler.Handle(reservationRequest, CancellationToken.None);

        Assert.Equal(1, stockItem.Reserved);
        Assert.Equal(1, stockItem.Available);
        var stockReservedEvent = inventoryOutbox.Events.OfType<OrderStockReservedEvent>().Single();

        var stockReservedHandler = new OrderStockReservedEventHandler(
            orderRepository,
            new FakeOrderingUnitOfWork(),
            orderingOutbox,
            inbox,
            NullLogger<OrderStockReservedEventHandler>.Instance);

        await stockReservedHandler.Handle(stockReservedEvent, CancellationToken.None);

        Assert.Equal(OrderStatus.Unpaid, order.Status);
        var orderCreatedEvent = orderingOutbox.Events.OfType<OrderCreatedEvent>().Single();

        var orderCreatedNotificationHandler = new OrderCreatedNotificationHandler(
            notificationOutbox,
            new FakeNotificationsUnitOfWork(),
            inbox,
            NullLogger<OrderCreatedNotificationHandler>.Instance);

        await orderCreatedNotificationHandler.Handle(orderCreatedEvent, CancellationToken.None);
        Assert.Contains(notificationOutbox.Messages, m => m.Subject.Contains("placed"));

        var paymentHandler = new MarkOrderPaidCommandHandler(
            orderRepository,
            new FakeOrderingUnitOfWork(),
            orderingOutbox,
            new FakePaymentGateway());

        var paymentResult = await paymentHandler.Handle(new MarkOrderPaidCommand(order.OrderId, customerId), CancellationToken.None);

        Assert.True(paymentResult.IsSuccess);
        Assert.Equal(OrderStatus.Paid, order.Status);
        var orderPaidEvent = orderingOutbox.Events.OfType<OrderPaidEvent>().Single();

        var orderPaidInventoryHandler = new OrderPaidEventHandler(
            stockRepository,
            new FakeInventoryUnitOfWork(),
            inventoryOutbox,
            inbox,
            NullLogger<OrderPaidEventHandler>.Instance);

        await orderPaidInventoryHandler.Handle(orderPaidEvent, CancellationToken.None);

        Assert.Equal(0, stockItem.Reserved);
        Assert.Equal(1, stockItem.Quantity);
        Assert.Equal(1, stockItem.Available);

        var orderPaidNotificationHandler = new OrderPaidNotificationHandler(
            notificationOutbox,
            new FakeNotificationsUnitOfWork(),
            inbox,
            NullLogger<OrderPaidNotificationHandler>.Instance);

        await orderPaidNotificationHandler.Handle(orderPaidEvent, CancellationToken.None);
        Assert.Contains(notificationOutbox.Messages, m => m.Subject.Contains("Payment confirmed"));
        Assert.Equal(2, notificationOutbox.Messages.Count);

        var eventCountBeforeReplay = orderingOutbox.Events.Count;
        await checkoutHandler.Handle(checkoutEvent, CancellationToken.None);

        Assert.Single(orderRepository.Orders);
        Assert.Equal(eventCountBeforeReplay, orderingOutbox.Events.Count);
    }

    private sealed class FakeOrderRepository(params Order[] orders) : IOrderRepository
    {
        public List<Order> Orders { get; } = orders.ToList();

        public Task<Result<Order>> GetByIdAsync(Guid orderId)
        {
            var order = Orders.SingleOrDefault(x => x.OrderId == orderId);
            return Task.FromResult(order is null
                ? Result.Failure<Order>("Order not found")
                : Result.Success(order));
        }

        public Task<Result<Order>> GetBySourceCheckoutIdAsync(Guid sourceCheckoutId)
        {
            var order = Orders.SingleOrDefault(x => x.SourceCheckoutId == sourceCheckoutId);
            return Task.FromResult(order is null
                ? Result.Failure<Order>("Order not found")
                : Result.Success(order));
        }

        public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId) =>
            Task.FromResult<IReadOnlyList<Order>>(Orders.Where(o => o.CustomerId == customerId).ToList());

        public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int skip, int take) =>
            Task.FromResult<IReadOnlyList<Order>>(Orders.Where(o => o.CustomerId == customerId).Skip(skip).Take(take).ToList());

        public Task<Result> AddAsync(Order order)
        {
            Orders.Add(order);
            return Task.FromResult(Result.Success());
        }

        public Task<Result> UpdateAsync(Order order) => Task.FromResult(Result.Success());
    }

    private sealed class FakeStockItemRepository(params StockItem[] stockItems) : IStockItemRepository
    {
        private readonly List<StockItem> _stockItems = stockItems.ToList();

        public Task<Result<StockItem>> GetByProductIdAsync(Guid productId)
        {
            var stockItem = _stockItems.SingleOrDefault(x => x.ProductId == productId);
            return Task.FromResult(stockItem is null
                ? Result.Failure<StockItem>("Stock item not found")
                : Result.Success(stockItem));
        }

        public Task<Result> AddAsync(StockItem stockItem)
        {
            _stockItems.Add(stockItem);
            return Task.FromResult(Result.Success());
        }

        public Task<Result> UpdateAsync(StockItem stockItem) => Task.FromResult(Result.Success());
    }

    private sealed class FakeDomainEventInbox :
        IOrderingDomainEventInbox,
        IInventoryDomainEventInbox,
        INotificationsDomainEventInbox
    {
        private readonly HashSet<(Guid EventId, string Consumer)> _processed = [];

        public Task<bool> HasProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken) =>
            Task.FromResult(_processed.Contains((eventId, consumer)));

        public void AddProcessed(IDomainEvent domainEvent, string consumer) =>
            _processed.Add((domainEvent.EventId, consumer));

        public void AddProcessed(Guid eventId, string eventType, string consumer) =>
            _processed.Add((eventId, consumer));
    }

    private sealed class FakeOrderingOutbox : IOrderingDomainEventOutbox
    {
        public List<IDomainEvent> Events { get; } = [];

        public Task AddAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            Events.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeInventoryOutbox : IInventoryDomainEventOutbox
    {
        public List<IDomainEvent> Events { get; } = [];

        public Task AddAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            Events.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeNotificationOutbox : INotificationOutbox
    {
        public List<NotificationMessage> Messages { get; } = [];

        public Task EnqueueAsync(string to, string subject, string body, string? dedupeKey, CancellationToken cancellationToken)
        {
            if (dedupeKey is not null && Messages.Any(m => m.DedupeKey == dedupeKey))
                return Task.CompletedTask;

            Messages.Add(new NotificationMessage(to, subject, body, dedupeKey));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeOrderingUnitOfWork : IOrderingUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class FakeInventoryUnitOfWork : IInventoryUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class FakeNotificationsUnitOfWork : INotificationsUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class FakePaymentGateway : IPaymentGateway
    {
        public Task<Result<PaymentResult>> CaptureAsync(
            Guid orderId,
            Guid customerId,
            string paymentMethod,
            decimal amount,
            CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success(new PaymentResult(amount, "flow-test-transaction")));
    }

    private sealed record NotificationMessage(string To, string Subject, string Body, string? DedupeKey);
}
