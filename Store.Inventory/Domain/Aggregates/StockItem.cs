using CSharpFunctionalExtensions;
using Store.Inventory.Contracts.Events;
using Store.SharedKernel;

namespace Store.Inventory.Domain.Aggregates;

public class StockItem : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int Reserved { get; private set; }
    public int Available => Quantity - Reserved;

    private StockItem() { }

    public static Result<StockItem> Create(Guid productId, int initialQuantity)
    {
        if (productId == Guid.Empty)
            return Result.Failure<StockItem>("ProductId is required");
        if (initialQuantity < 0)
            return Result.Failure<StockItem>("Initial quantity cannot be negative");

        return Result.Success(new StockItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = initialQuantity,
            Reserved = 0
        });
    }

    internal static StockItem Reconstitute(Guid id, Guid productId, int quantity, int reserved) =>
        new() { Id = id, ProductId = productId, Quantity = quantity, Reserved = reserved };

    public Result Replenish(int amount)
    {
        if (amount <= 0)
            return Result.Failure("Amount must be positive");

        Quantity += amount;
        return Result.Success();
    }

    public Result Reserve(int amount)
    {
        if (amount <= 0)
            return Result.Failure("Amount must be positive");
        if (Available < amount)
            return Result.Failure($"Insufficient stock: available {Available}, requested {amount}");

        Reserved += amount;

        if (Available == 0)
            RaiseDomainEvent(new StockDepletedEvent(ProductId));

        return Result.Success();
    }

    public Result Release(int amount)
    {
        if (amount <= 0)
            return Result.Failure("Amount must be positive");
        if (Reserved < amount)
            return Result.Failure($"Cannot release {amount}: only {Reserved} reserved");

        Reserved -= amount;
        return Result.Success();
    }

    public Result Deduct(int amount)
    {
        if (amount <= 0)
            return Result.Failure("Amount must be positive");
        if (Reserved < amount)
            return Result.Failure($"Cannot deduct {amount}: only {Reserved} reserved");

        Reserved -= amount;
        Quantity -= amount;
        return Result.Success();
    }
}
