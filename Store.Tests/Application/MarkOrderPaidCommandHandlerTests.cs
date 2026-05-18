using CSharpFunctionalExtensions;
using Store.Ordering.Application.CQRS.Command;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Contracts.Events;
using Store.Ordering.Domain.Aggregates;
using Store.Ordering.Domain.Interfaces;
using Store.Ordering.Domain.ValueObjects;
using Store.SharedKernel.Events;

namespace Store.Tests.Application;

public class MarkOrderPaidCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCustomerOwnsOrder_MarksOrderPaidAndPublishesEvent()
    {
        var order = CreateUnpaidOrder(Guid.NewGuid());
        var repository = new FakeOrderRepository(order);
        var outbox = new FakeOrderingDomainEventOutbox();
        var handler = new MarkOrderPaidCommandHandler(repository, new FakeOrderingUnitOfWork(), outbox);

        var result = await handler.Handle(new MarkOrderPaidCommand(order.OrderId, order.CustomerId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Store.Ordering.Domain.Enums.OrderStatus.Paid, order.Status);
        Assert.Single(outbox.DomainEvents.OfType<OrderPaidEvent>());
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotOwnOrder_ReturnsAccessDenied()
    {
        var order = CreateUnpaidOrder(Guid.NewGuid());
        var repository = new FakeOrderRepository(order);
        var outbox = new FakeOrderingDomainEventOutbox();
        var handler = new MarkOrderPaidCommandHandler(repository, new FakeOrderingUnitOfWork(), outbox);

        var result = await handler.Handle(new MarkOrderPaidCommand(order.OrderId, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Access denied", result.Error);
        Assert.Empty(outbox.DomainEvents);
    }

    private static Order CreateUnpaidOrder(Guid customerId)
    {
        var product = OrderedProduct.Create(Guid.NewGuid(), "Keyboard", 99.9m, 1).Value;
        var order = Order.Create(
            customerId,
            "customer@example.com",
            "Ivan Petrov",
            "+79990000000",
            "Tomsk, Lenina 1",
            "Courier",
            "Card",
            [product],
            Guid.NewGuid()).Value;

        order.ConfirmStockReserved();
        order.ClearDomainEvents();
        return order;
    }

    private sealed class FakeOrderRepository(params Order[] orders) : IOrderRepository
    {
        private readonly List<Order> _orders = orders.ToList();

        public Task<Result<Order>> GetByIdAsync(Guid orderId)
        {
            var order = _orders.SingleOrDefault(x => x.OrderId == orderId);
            return Task.FromResult(order is null
                ? Result.Failure<Order>("Order not found")
                : Result.Success(order));
        }

        public Task<Result<Order>> GetBySourceCheckoutIdAsync(Guid sourceCheckoutId) =>
            Task.FromResult(Result.Failure<Order>("Order not found"));

        public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId) =>
            Task.FromResult<IReadOnlyList<Order>>(_orders.Where(o => o.CustomerId == customerId).ToList());

        public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int skip, int take) =>
            Task.FromResult<IReadOnlyList<Order>>(_orders.Where(o => o.CustomerId == customerId).Skip(skip).Take(take).ToList());

        public Task<Result> AddAsync(Order order)
        {
            _orders.Add(order);
            return Task.FromResult(Result.Success());
        }

        public Task<Result> UpdateAsync(Order order) =>
            Task.FromResult(Result.Success());
    }

    private sealed class FakeOrderingUnitOfWork : IOrderingUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(1);
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
}
