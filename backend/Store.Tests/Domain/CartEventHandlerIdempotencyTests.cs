using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Store.Carts.Application.EventHandlers;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Interfaces;
using Store.Ordering.Contracts.Events;
using Store.SharedKernel.Events;

namespace Store.Tests.Domain;

public class CartEventHandlerIdempotencyTests
{
    [Fact]
    public async Task OrderCreatedHandler_WhenEventIsReplayed_DoesNotCompleteCheckoutTwice()
    {
        var cart = CreatePendingCart();
        var repository = new FakeCartRepository(cart);
        var unitOfWork = new FakeCartUnitOfWork();
        var inbox = new FakeCartDomainEventInbox();
        var handler = new OrderCreatedEventHandler(
            repository,
            unitOfWork,
            inbox,
            new FakeCheckoutOrderRecorder(),
            NullLogger<OrderCreatedEventHandler>.Instance);
        var notification = CreateOrderCreatedEvent(cart);

        await handler.Handle(notification, CancellationToken.None);
        await handler.Handle(notification, CancellationToken.None);

        Assert.False(cart.IsCheckoutPending);
        Assert.Empty(cart.Items);
        Assert.Equal(1, repository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Contains(notification.EventId, inbox.ProcessedEventIds);
    }

    [Fact]
    public async Task OrderRejectedHandler_WhenEventIsReplayed_DoesNotReleaseCheckoutTwice()
    {
        var cart = CreatePendingCart();
        var repository = new FakeCartRepository(cart);
        var unitOfWork = new FakeCartUnitOfWork();
        var inbox = new FakeCartDomainEventInbox();
        var handler = new OrderRejectedEventHandler(
            repository,
            unitOfWork,
            inbox,
            NullLogger<OrderRejectedEventHandler>.Instance);
        var notification = CreateOrderRejectedEvent(cart.CustomerId);

        await handler.Handle(notification, CancellationToken.None);
        await handler.Handle(notification, CancellationToken.None);

        Assert.False(cart.IsCheckoutPending);
        Assert.Single(cart.Items);
        Assert.Equal(1, repository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Contains(notification.EventId, inbox.ProcessedEventIds);
    }

    [Fact]
    public async Task OrderCancelledHandler_WhenCheckoutMatches_ReleasesCheckoutAndKeepsItems()
    {
        var cart = CreatePendingCart();
        var repository = new FakeCartRepository(cart);
        var unitOfWork = new FakeCartUnitOfWork();
        var inbox = new FakeCartDomainEventInbox();
        var handler = new OrderCancelledEventHandler(
            repository,
            unitOfWork,
            inbox,
            NullLogger<OrderCancelledEventHandler>.Instance);
        var notification = CreateOrderCancelledEvent(cart);

        await handler.Handle(notification, CancellationToken.None);
        await handler.Handle(notification, CancellationToken.None);

        Assert.False(cart.IsCheckoutPending);
        Assert.Single(cart.Items);
        Assert.Equal(1, repository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Contains(notification.EventId, inbox.ProcessedEventIds);
    }

    [Fact]
    public async Task OrderCancelledHandler_WhenCheckoutDoesNotMatch_DoesNotReleaseCurrentCheckout()
    {
        var cart = CreatePendingCart();
        var repository = new FakeCartRepository(cart);
        var unitOfWork = new FakeCartUnitOfWork();
        var inbox = new FakeCartDomainEventInbox();
        var handler = new OrderCancelledEventHandler(
            repository,
            unitOfWork,
            inbox,
            NullLogger<OrderCancelledEventHandler>.Instance);
        var notification = CreateOrderCancelledEvent(cart, Guid.NewGuid());

        await handler.Handle(notification, CancellationToken.None);

        Assert.True(cart.IsCheckoutPending);
        Assert.Single(cart.Items);
        Assert.Equal(0, repository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Contains(notification.EventId, inbox.ProcessedEventIds);
    }

    [Fact]
    public async Task OrderCreatedHandler_WhenUpdateFails_DoesNotMarkEventProcessed()
    {
        var cart = CreatePendingCart();
        var repository = new FakeCartRepository(cart) { UpdateResult = Result.Failure("update failed") };
        var unitOfWork = new FakeCartUnitOfWork();
        var inbox = new FakeCartDomainEventInbox();
        var handler = new OrderCreatedEventHandler(
            repository,
            unitOfWork,
            inbox,
            new FakeCheckoutOrderRecorder(),
            NullLogger<OrderCreatedEventHandler>.Instance);
        var notification = CreateOrderCreatedEvent(cart);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(notification, CancellationToken.None));

        Assert.Contains("update failed", exception.Message);
        Assert.DoesNotContain(notification.EventId, inbox.ProcessedEventIds);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static Cart CreatePendingCart()
    {
        var cart = Cart.Create(Guid.NewGuid()).Value;
        cart.AddItem(Guid.NewGuid(), "Keyboard", 99.9m, 1);
        cart.Checkout(
            "customer@example.com",
            "Ivan Petrov",
            "+79990000000",
            "Tomsk, Lenina 1",
            "Courier",
            "Card");
        cart.ClearDomainEvents();
        return cart;
    }

    private static OrderCreatedEvent CreateOrderCreatedEvent(Cart cart) =>
        new(
            Guid.NewGuid(),
            cart.PendingCheckoutId,
            cart.CustomerId,
            "customer@example.com",
            [new OrderItem(Guid.NewGuid(), "Keyboard", 99.9m, 1)]);

    private static OrderRejectedEvent CreateOrderRejectedEvent(Guid customerId) =>
        new(
            Guid.NewGuid(),
            null,
            customerId,
            "customer@example.com",
            "Insufficient stock",
            [new OrderItem(Guid.NewGuid(), "Keyboard", 99.9m, 1)]);

    private static OrderCancelledEvent CreateOrderCancelledEvent(Cart cart, Guid? sourceCheckoutId = null) =>
        new(
            Guid.NewGuid(),
            cart.CustomerId,
            "customer@example.com",
            [new OrderItem(Guid.NewGuid(), "Keyboard", 99.9m, 1)],
            sourceCheckoutId ?? cart.PendingCheckoutId);

    private sealed class FakeCartRepository(Cart cart) : ICartRepository
    {
        public int UpdateCallCount { get; private set; }
        public Result UpdateResult { get; init; } = Result.Success();

        public Task<Result<Cart>> GetByCustomerIdAsync(Guid customerId) =>
            Task.FromResult(customerId == cart.CustomerId
                ? Result.Success(cart)
                : Result.Failure<Cart>("Cart not found"));

        public Task<IReadOnlyList<Cart>> GetPendingCheckoutsStartedBeforeAsync(DateTimeOffset staleBefore) =>
            Task.FromResult<IReadOnlyList<Cart>>(
                cart.CheckoutPendingSince <= staleBefore ? [cart] : []);

        public Task<Result> AddAsync(Cart cart) => Task.FromResult(Result.Success());

        public Task<Result> UpdateAsync(Cart cart)
        {
            UpdateCallCount++;
            return Task.FromResult(UpdateResult);
        }
    }

    private sealed class FakeCartUnitOfWork : ICartUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeCartDomainEventInbox : ICartDomainEventInbox
    {
        public List<Guid> ProcessedEventIds { get; } = [];

        public Task<bool> HasProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken) =>
            Task.FromResult(ProcessedEventIds.Contains(eventId));

        public void AddProcessed(IDomainEvent domainEvent, string consumer) =>
            ProcessedEventIds.Add(domainEvent.EventId);

        public void AddProcessed(Guid eventId, string eventType, string consumer) =>
            ProcessedEventIds.Add(eventId);
    }

    private sealed class FakeCheckoutOrderRecorder : ICheckoutOrderRecorder
    {
        public Task RecordOrderCreatedAsync(
            Guid checkoutId,
            Guid orderId,
            Guid customerId,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
