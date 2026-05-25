using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Store.Carts.Contracts.Events;
using Store.Notifications.Application.EventHandlers;
using Store.Notifications.Application.Interfaces;
using Store.Inventory.Contracts.Events;
using Store.Ordering.Application.EventHandlers;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Contracts.Events;
using Store.Ordering.Domain.Aggregates;
using Store.Ordering.Domain.Enums;
using Store.Ordering.Domain.Interfaces;
using Store.Ordering.Domain.ValueObjects;
using Store.SharedKernel.Events;

namespace Store.Tests.Domain;

public class EventIdempotencyTests
{
    [Fact]
    public async Task CartCheckedOutHandler_WhenEventIsReplayed_DoesNotCreateSecondOrder()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeOrderingUnitOfWork();
        var outbox = new FakeOrderingDomainEventOutbox();
        var inbox = new FakeOrderingDomainEventInbox();
        var handler = new CartCheckedOutEventHandler(
            repository,
            unitOfWork,
            outbox,
            inbox,
            NullLogger<CartCheckedOutEventHandler>.Instance);

        var notification = new CartCheckedOutEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Ivan Petrov",
            "+79990000000",
            "Tomsk, Lenina 1",
            "Courier",
            "Card",
            [new CartCheckedOutItem(Guid.NewGuid(), "Keyboard", 99.9m, 1)]);

        await handler.Handle(notification, CancellationToken.None);
        await handler.Handle(notification, CancellationToken.None);

        Assert.Single(repository.Orders);
        Assert.Equal(notification.EventId, repository.Orders.Single().SourceCheckoutId);
        Assert.Single(outbox.DomainEvents.OfType<OrderStockReservationRequestedEvent>());
    }

    [Fact]
    public async Task OrderStockReservedHandler_WhenReservationSucceeds_PublishesOrderCreated()
    {
        var repository = new FakeOrderRepository();
        var order = CreateAwaitingStockOrder();
        repository.Orders.Add(order);
        order.ClearDomainEvents();
        var unitOfWork = new FakeOrderingUnitOfWork();
        var outbox = new FakeOrderingDomainEventOutbox();
        var inbox = new FakeOrderingDomainEventInbox();
        var handler = new OrderStockReservedEventHandler(
            repository,
            unitOfWork,
            outbox,
            inbox,
            NullLogger<OrderStockReservedEventHandler>.Instance);

        await handler.Handle(new OrderStockReservedEvent(
            order.OrderId,
            order.CustomerId,
            order.CustomerEmail,
            order.Products.Select(p => new ReservedStockItem(p.ProductId, p.ProductName, p.Price, p.Quantity)).ToList()), CancellationToken.None);

        Assert.Equal(OrderStatus.Unpaid, order.Status);
        Assert.Single(outbox.DomainEvents.OfType<OrderCreatedEvent>());
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task OrderStockReservationRejectedHandler_WhenReservationFails_PublishesOrderRejected()
    {
        var repository = new FakeOrderRepository();
        var order = CreateAwaitingStockOrder();
        repository.Orders.Add(order);
        order.ClearDomainEvents();
        var unitOfWork = new FakeOrderingUnitOfWork();
        var outbox = new FakeOrderingDomainEventOutbox();
        var inbox = new FakeOrderingDomainEventInbox();
        var handler = new OrderStockReservationRejectedEventHandler(
            repository,
            unitOfWork,
            outbox,
            inbox,
            NullLogger<OrderStockReservationRejectedEventHandler>.Instance);

        await handler.Handle(new OrderStockReservationRejectedEvent(
            order.OrderId,
            order.CustomerId,
            order.CustomerEmail,
            "Insufficient stock",
            order.Products.Select(p => new ReservedStockItem(p.ProductId, p.ProductName, p.Price, p.Quantity)).ToList()), CancellationToken.None);

        Assert.Equal(OrderStatus.Rejected, order.Status);
        Assert.Single(outbox.DomainEvents.OfType<OrderRejectedEvent>());
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task OrderCreatedNotificationHandler_WhenEventIsReplayed_DoesNotCreateSecondEmail()
    {
        var outbox = new FakeNotificationOutbox();
        var unitOfWork = new FakeNotificationsUnitOfWork();
        var inbox = new FakeNotificationsDomainEventInbox();
        var handler = new OrderCreatedNotificationHandler(
            outbox,
            unitOfWork,
            inbox,
            NullLogger<OrderCreatedNotificationHandler>.Instance);

        var notification = CreateOrderCreatedEvent();

        await handler.Handle(notification, CancellationToken.None);
        await handler.Handle(notification, CancellationToken.None);

        Assert.Single(outbox.DedupeKeys);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task OrderCreatedNotificationHandler_WhenOutboxFails_Rethrows()
    {
        var outbox = new FakeNotificationOutbox { ThrowOnEnqueue = true };
        var handler = new OrderCreatedNotificationHandler(
            outbox,
            new FakeNotificationsUnitOfWork(),
            new FakeNotificationsDomainEventInbox(),
            NullLogger<OrderCreatedNotificationHandler>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(CreateOrderCreatedEvent(), CancellationToken.None));
    }

    private static OrderCreatedEvent CreateOrderCreatedEvent() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            [new OrderItem(Guid.NewGuid(), "Keyboard", 99.9m, 1)]);

    private static Order CreateAwaitingStockOrder()
    {
        var product = OrderedProduct.Create(Guid.NewGuid(), "Keyboard", 99.9m, 1).Value;
        return Order.Create(
            Guid.NewGuid(),
            "customer@example.com",
            "Ivan Petrov",
            "+79990000000",
            "Card",
            [product],
            Guid.NewGuid()).Value;
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public List<Order> Orders { get; } = [];

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
            Task.FromResult<IReadOnlyList<Order>>([]);

        public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int skip, int take) =>
            Task.FromResult<IReadOnlyList<Order>>([]);

        public Task<Result> AddAsync(Order order)
        {
            Orders.Add(order);
            return Task.FromResult(Result.Success());
        }

        public Task<Result> UpdateAsync(Order order) =>
            Task.FromResult(Result.Success());
    }

    private sealed class FakeOrderingUnitOfWork : IOrderingUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(++SaveChangesCallCount);
    }

    private sealed class FakeOrderingDomainEventOutbox : IOrderingDomainEventOutbox
    {
        public List<IDomainEvent> DomainEvents { get; } = [];

        public Task AddAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            DomainEvents.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeOrderingDomainEventInbox : IOrderingDomainEventInbox
    {
        private readonly HashSet<(Guid EventId, string Consumer)> _processed = [];

        public Task<bool> HasProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken) =>
            Task.FromResult(_processed.Contains((eventId, consumer)));

        public void AddProcessed(IDomainEvent domainEvent, string consumer) =>
            AddProcessed(domainEvent.EventId, domainEvent.EventType, consumer);

        public void AddProcessed(Guid eventId, string eventType, string consumer)
        {
            _processed.Add((eventId, consumer));
        }
    }

    private sealed class FakeNotificationOutbox : INotificationOutbox
    {
        public List<string?> DedupeKeys { get; } = [];
        public bool ThrowOnEnqueue { get; init; }

        public Task EnqueueAsync(
            string to,
            string subject,
            string body,
            string? dedupeKey,
            CancellationToken cancellationToken)
        {
            if (ThrowOnEnqueue)
                throw new InvalidOperationException("Outbox failed");

            DedupeKeys.Add(dedupeKey);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeNotificationsUnitOfWork : INotificationsUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeNotificationsDomainEventInbox : INotificationsDomainEventInbox
    {
        private readonly HashSet<(Guid EventId, string Consumer)> _processed = [];

        public Task<bool> HasProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken) =>
            Task.FromResult(_processed.Contains((eventId, consumer)));

        public void AddProcessed(IDomainEvent domainEvent, string consumer) =>
            AddProcessed(domainEvent.EventId, domainEvent.EventType, consumer);

        public void AddProcessed(Guid eventId, string eventType, string consumer)
        {
            _processed.Add((eventId, consumer));
        }
    }
}
