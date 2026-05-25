using CSharpFunctionalExtensions;
using Store.Carts.Application.CQRS.Command;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Interfaces;

namespace Store.Tests.Application;

public class ReleaseStaleCheckoutsCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenPendingCheckoutIsFresh_DoesNotReleaseIt()
    {
        var now = DateTimeOffset.UtcNow;
        var cart = CreatePendingCart(now.AddMinutes(-5));
        var repository = new FakeCartRepository([cart]);
        var handler = new ReleaseStaleCheckoutsCommandHandler(
            repository,
            new FakeCheckoutOrderLookup(),
            new FakeCartUnitOfWork());

        var result = await handler.Handle(
            new ReleaseStaleCheckoutsCommand { StaleBefore = now.AddMinutes(-30) },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.CheckedCount);
        Assert.True(cart.IsCheckoutPending);
        Assert.Equal(0, repository.UpdateCallCount);
    }

    [Fact]
    public async Task Handle_WhenPendingCheckoutIsStaleAndOrderDoesNotExist_ReleasesIt()
    {
        var now = DateTimeOffset.UtcNow;
        var cart = CreatePendingCart(now.AddHours(-2));
        var repository = new FakeCartRepository([cart]);
        var unitOfWork = new FakeCartUnitOfWork();
        var handler = new ReleaseStaleCheckoutsCommandHandler(
            repository,
            new FakeCheckoutOrderLookup(),
            unitOfWork);

        var result = await handler.Handle(
            new ReleaseStaleCheckoutsCommand { StaleBefore = now.AddMinutes(-30) },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.CheckedCount);
        Assert.Equal(1, result.Value.ReleasedCount);
        Assert.False(cart.IsCheckoutPending);
        Assert.Null(cart.PendingCheckoutId);
        Assert.Null(cart.CheckoutPendingSince);
        Assert.Equal(1, repository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPendingCheckoutIsStaleButOrderExists_KeepsItPending()
    {
        var now = DateTimeOffset.UtcNow;
        var cart = CreatePendingCart(now.AddHours(-2));
        var repository = new FakeCartRepository([cart]);
        var lookup = new FakeCheckoutOrderLookup { ExistingCheckoutIds = [cart.PendingCheckoutId!.Value] };
        var handler = new ReleaseStaleCheckoutsCommandHandler(
            repository,
            lookup,
            new FakeCartUnitOfWork());

        var result = await handler.Handle(
            new ReleaseStaleCheckoutsCommand { StaleBefore = now.AddMinutes(-30) },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.CheckedCount);
        Assert.Equal(0, result.Value.ReleasedCount);
        Assert.Equal(1, result.Value.SkippedBecauseOrderExists);
        Assert.True(cart.IsCheckoutPending);
        Assert.Equal(0, repository.UpdateCallCount);
    }

    private static Cart CreatePendingCart(DateTimeOffset checkoutStartedAt)
    {
        var cart = Cart.Create(Guid.NewGuid()).Value;
        cart.AddItem(Guid.NewGuid(), "Keyboard", 99.9m, 1);
        cart.Checkout(
            "customer@example.com",
            "Ivan Petrov",
            "+79990000000",
            "Tomsk, Lenina 1",
            "Courier",
            "Card",
            checkoutStartedAt);
        cart.ClearDomainEvents();
        return cart;
    }

    private sealed class FakeCartRepository(IReadOnlyList<Cart> carts) : ICartRepository
    {
        public int UpdateCallCount { get; private set; }

        public Task<Result<Cart>> GetByCustomerIdAsync(Guid customerId)
        {
            var cart = carts.SingleOrDefault(c => c.CustomerId == customerId);
            return Task.FromResult(cart is null
                ? Result.Failure<Cart>("Cart not found")
                : Result.Success(cart));
        }

        public Task<IReadOnlyList<Cart>> GetPendingCheckoutsStartedBeforeAsync(DateTimeOffset staleBefore) =>
            Task.FromResult<IReadOnlyList<Cart>>(carts
                .Where(c => c.IsCheckoutPending && c.CheckoutPendingSince <= staleBefore)
                .ToList());

        public Task<Result> AddAsync(Cart cart) => Task.FromResult(Result.Success());

        public Task<Result> UpdateAsync(Cart cart)
        {
            UpdateCallCount++;
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FakeCheckoutOrderLookup : ICheckoutOrderLookup
    {
        public HashSet<Guid> ExistingCheckoutIds { get; init; } = [];

        public Task<bool> HasOrderForCheckoutAsync(Guid checkoutId, CancellationToken cancellationToken) =>
            Task.FromResult(ExistingCheckoutIds.Contains(checkoutId));
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
}
