using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Store.Inventory.Application.EventHandlers;
using Store.Inventory.Application.Interfaces;
using Store.Inventory.Contracts.Events;
using Store.Inventory.Domain.Aggregates;
using Store.Inventory.Domain.Interfaces;
using Store.Ordering.Contracts.Events;
using Store.SharedKernel.Events;

namespace Store.Tests.Domain;

public class InventoryTests
{
    [Fact]
    public void Create_WithValidProductAndQuantity_InitializesStock()
    {
        var productId = Guid.NewGuid();

        var result = StockItem.Create(productId, 10);

        Assert.True(result.IsSuccess);
        var stockItem = result.Value;
        Assert.NotEqual(Guid.Empty, stockItem.Id);
        Assert.Equal(productId, stockItem.ProductId);
        Assert.Equal(10, stockItem.Quantity);
        Assert.Equal(0, stockItem.Reserved);
        Assert.Equal(10, stockItem.Available);
        Assert.Empty(stockItem.DomainEvents);
    }

    [Fact]
    public void Create_WithEmptyProductId_Fails()
    {
        var result = StockItem.Create(Guid.Empty, 10);

        Assert.True(result.IsFailure);
        Assert.Equal("ProductId is required", result.Error);
    }

    [Fact]
    public void Create_WithNegativeInitialQuantity_Fails()
    {
        var result = StockItem.Create(Guid.NewGuid(), -1);

        Assert.True(result.IsFailure);
        Assert.Equal("Initial quantity cannot be negative", result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Replenish_WithNonPositiveAmount_FailsWithoutChangingQuantity(int amount)
    {
        var stockItem = CreateStockItem(initialQuantity: 10);

        var result = stockItem.Replenish(amount);

        Assert.True(result.IsFailure);
        Assert.Equal("Amount must be positive", result.Error);
        Assert.Equal(10, stockItem.Quantity);
        Assert.Equal(10, stockItem.Available);
    }

    [Fact]
    public void Replenish_WithPositiveAmount_IncreasesQuantityAndAvailableStock()
    {
        var stockItem = CreateStockItem(initialQuantity: 10);
        stockItem.Reserve(4);

        var result = stockItem.Replenish(5);

        Assert.True(result.IsSuccess);
        Assert.Equal(15, stockItem.Quantity);
        Assert.Equal(4, stockItem.Reserved);
        Assert.Equal(11, stockItem.Available);
    }

    [Fact]
    public void Reserve_WhenEnoughStock_ReservesQuantityWithoutDomainEvent()
    {
        var stockItem = CreateStockItem(initialQuantity: 10);

        var result = stockItem.Reserve(4);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, stockItem.Quantity);
        Assert.Equal(4, stockItem.Reserved);
        Assert.Equal(6, stockItem.Available);
        Assert.Empty(stockItem.DomainEvents);
    }

    [Fact]
    public void Reserve_WhenAllAvailableStockIsReserved_RaisesStockDepletedEvent()
    {
        var productId = Guid.NewGuid();
        var stockItem = CreateStockItem(productId, initialQuantity: 5);

        var result = stockItem.Reserve(5);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, stockItem.Available);

        var domainEvent = Assert.IsType<StockDepletedEvent>(Assert.Single(stockItem.DomainEvents));
        Assert.Equal(productId, domainEvent.ProductId);
    }

    [Fact]
    public void Reserve_WhenInsufficientStock_FailsWithoutChangingReservation()
    {
        var stockItem = CreateStockItem(initialQuantity: 3);

        var result = stockItem.Reserve(4);

        Assert.True(result.IsFailure);
        Assert.Equal("Insufficient stock: available 3, requested 4", result.Error);
        Assert.Equal(0, stockItem.Reserved);
        Assert.Equal(3, stockItem.Available);
        Assert.Empty(stockItem.DomainEvents);
    }

    [Fact]
    public void Release_WhenQuantityIsReserved_DecreasesReservedQuantity()
    {
        var stockItem = CreateStockItem(initialQuantity: 10);
        stockItem.Reserve(6);

        var result = stockItem.Release(4);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, stockItem.Reserved);
        Assert.Equal(8, stockItem.Available);
    }

    [Fact]
    public void Release_WhenAmountExceedsReservedQuantity_FailsWithoutChangingStock()
    {
        var stockItem = CreateStockItem(initialQuantity: 10);
        stockItem.Reserve(3);

        var result = stockItem.Release(4);

        Assert.True(result.IsFailure);
        Assert.Equal("Cannot release 4: only 3 reserved", result.Error);
        Assert.Equal(10, stockItem.Quantity);
        Assert.Equal(3, stockItem.Reserved);
        Assert.Equal(7, stockItem.Available);
    }

    [Fact]
    public void Deduct_WhenQuantityIsReserved_DecreasesReservedAndTotalQuantity()
    {
        var stockItem = CreateStockItem(initialQuantity: 10);
        stockItem.Reserve(6);

        var result = stockItem.Deduct(4);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, stockItem.Quantity);
        Assert.Equal(2, stockItem.Reserved);
        Assert.Equal(4, stockItem.Available);
    }

    [Fact]
    public void Deduct_WhenAmountExceedsReservedQuantity_FailsWithoutChangingStock()
    {
        var stockItem = CreateStockItem(initialQuantity: 10);
        stockItem.Reserve(3);

        var result = stockItem.Deduct(4);

        Assert.True(result.IsFailure);
        Assert.Equal("Cannot deduct 4: only 3 reserved", result.Error);
        Assert.Equal(10, stockItem.Quantity);
        Assert.Equal(3, stockItem.Reserved);
        Assert.Equal(7, stockItem.Available);
    }

    [Fact]
    public async Task OrderCreatedHandler_ReservesTrackedItemsPersistsEventsAndClearsDomainEvents()
    {
        var productId = Guid.NewGuid();
        var stockItem = CreateStockItem(productId, initialQuantity: 2);
        var repository = new FakeStockItemRepository(stockItem);
        var unitOfWork = new FakeInventoryUnitOfWork();
        var outbox = new FakeInventoryDomainEventOutbox();
        var handler = new OrderCreatedEventHandler(
            repository,
            unitOfWork,
            outbox,
            NullLogger<OrderCreatedEventHandler>.Instance);

        await handler.Handle(CreateOrderCreatedEvent(productId, quantity: 2), CancellationToken.None);

        Assert.Equal(2, stockItem.Quantity);
        Assert.Equal(2, stockItem.Reserved);
        Assert.Equal(0, stockItem.Available);
        Assert.Contains(productId, repository.UpdatedProductIds);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var domainEvent = Assert.IsType<StockDepletedEvent>(Assert.Single(outbox.DomainEvents));
        Assert.Equal(productId, domainEvent.ProductId);
        Assert.Empty(stockItem.DomainEvents);
    }

    [Fact]
    public async Task OrderCreatedHandler_WhenStockIsMissing_SkipsReservationButStillSaves()
    {
        var productId = Guid.NewGuid();
        var repository = new FakeStockItemRepository();
        var unitOfWork = new FakeInventoryUnitOfWork();
        var outbox = new FakeInventoryDomainEventOutbox();
        var handler = new OrderCreatedEventHandler(
            repository,
            unitOfWork,
            outbox,
            NullLogger<OrderCreatedEventHandler>.Instance);

        await handler.Handle(CreateOrderCreatedEvent(productId, quantity: 2), CancellationToken.None);

        Assert.Empty(repository.UpdatedProductIds);
        Assert.Empty(outbox.DomainEvents);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task OrderPaidHandler_DeductsReservedItemsAndSaves()
    {
        var productId = Guid.NewGuid();
        var stockItem = CreateStockItem(productId, initialQuantity: 10);
        stockItem.Reserve(4);
        var repository = new FakeStockItemRepository(stockItem);
        var unitOfWork = new FakeInventoryUnitOfWork();
        var outbox = new FakeInventoryDomainEventOutbox();
        var handler = new OrderPaidEventHandler(
            repository,
            unitOfWork,
            outbox,
            NullLogger<OrderPaidEventHandler>.Instance);

        await handler.Handle(CreateOrderPaidEvent(productId, quantity: 4), CancellationToken.None);

        Assert.Equal(6, stockItem.Quantity);
        Assert.Equal(0, stockItem.Reserved);
        Assert.Equal(6, stockItem.Available);
        Assert.Contains(productId, repository.UpdatedProductIds);
        Assert.Empty(outbox.DomainEvents);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task OrderPaidHandler_WhenQuantityIsNotReserved_DoesNotUpdateStock()
    {
        var productId = Guid.NewGuid();
        var stockItem = CreateStockItem(productId, initialQuantity: 10);
        stockItem.Reserve(2);
        var repository = new FakeStockItemRepository(stockItem);
        var unitOfWork = new FakeInventoryUnitOfWork();
        var outbox = new FakeInventoryDomainEventOutbox();
        var handler = new OrderPaidEventHandler(
            repository,
            unitOfWork,
            outbox,
            NullLogger<OrderPaidEventHandler>.Instance);

        await handler.Handle(CreateOrderPaidEvent(productId, quantity: 3), CancellationToken.None);

        Assert.Equal(10, stockItem.Quantity);
        Assert.Equal(2, stockItem.Reserved);
        Assert.DoesNotContain(productId, repository.UpdatedProductIds);
        Assert.Empty(outbox.DomainEvents);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task OrderCancelledHandler_ReleasesReservedItemsAndSaves()
    {
        var productId = Guid.NewGuid();
        var stockItem = CreateStockItem(productId, initialQuantity: 10);
        stockItem.Reserve(4);
        var repository = new FakeStockItemRepository(stockItem);
        var unitOfWork = new FakeInventoryUnitOfWork();
        var outbox = new FakeInventoryDomainEventOutbox();
        var handler = new OrderCancelledEventHandler(
            repository,
            unitOfWork,
            outbox,
            NullLogger<OrderCancelledEventHandler>.Instance);

        await handler.Handle(CreateOrderCancelledEvent(productId, quantity: 4), CancellationToken.None);

        Assert.Equal(10, stockItem.Quantity);
        Assert.Equal(0, stockItem.Reserved);
        Assert.Equal(10, stockItem.Available);
        Assert.Contains(productId, repository.UpdatedProductIds);
        Assert.Empty(outbox.DomainEvents);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task OrderCancelledHandler_WhenQuantityIsNotReserved_DoesNotUpdateStock()
    {
        var productId = Guid.NewGuid();
        var stockItem = CreateStockItem(productId, initialQuantity: 10);
        stockItem.Reserve(2);
        var repository = new FakeStockItemRepository(stockItem);
        var unitOfWork = new FakeInventoryUnitOfWork();
        var outbox = new FakeInventoryDomainEventOutbox();
        var handler = new OrderCancelledEventHandler(
            repository,
            unitOfWork,
            outbox,
            NullLogger<OrderCancelledEventHandler>.Instance);

        await handler.Handle(CreateOrderCancelledEvent(productId, quantity: 3), CancellationToken.None);

        Assert.Equal(10, stockItem.Quantity);
        Assert.Equal(2, stockItem.Reserved);
        Assert.DoesNotContain(productId, repository.UpdatedProductIds);
        Assert.Empty(outbox.DomainEvents);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private static StockItem CreateStockItem(int initialQuantity) =>
        CreateStockItem(Guid.NewGuid(), initialQuantity);

    private static StockItem CreateStockItem(Guid productId, int initialQuantity) =>
        StockItem.Create(productId, initialQuantity).Value;

    private static OrderCreatedEvent CreateOrderCreatedEvent(Guid productId, int quantity) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            [new OrderItem(productId, "Keyboard", 99.9m, quantity)]);

    private static OrderPaidEvent CreateOrderPaidEvent(Guid productId, int quantity) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            [new OrderItem(productId, "Keyboard", 99.9m, quantity)]);

    private static OrderCancelledEvent CreateOrderCancelledEvent(Guid productId, int quantity) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            [new OrderItem(productId, "Keyboard", 99.9m, quantity)]);

    private sealed class FakeStockItemRepository(params StockItem[] stockItems) : IStockItemRepository
    {
        private readonly Dictionary<Guid, StockItem> _stockItems = stockItems.ToDictionary(item => item.ProductId);

        public List<Guid> UpdatedProductIds { get; } = [];

        public Task<Result<StockItem>> GetByProductIdAsync(Guid productId) =>
            Task.FromResult(_stockItems.TryGetValue(productId, out var stockItem)
                ? Result.Success(stockItem)
                : Result.Failure<StockItem>("Stock item not found"));

        public Task<Result> AddAsync(StockItem stockItem)
        {
            _stockItems[stockItem.ProductId] = stockItem;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> UpdateAsync(StockItem stockItem)
        {
            UpdatedProductIds.Add(stockItem.ProductId);
            _stockItems[stockItem.ProductId] = stockItem;
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FakeInventoryUnitOfWork : IInventoryUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeInventoryDomainEventOutbox : IInventoryDomainEventOutbox
    {
        public List<IDomainEvent> DomainEvents { get; } = [];

        public Task AddAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            DomainEvents.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }
}
