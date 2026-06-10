using CSharpFunctionalExtensions;
using Store.Inventory.Contracts.Events;
using Store.Inventory.Domain.Entities;
using Store.SharedKernel;

namespace Store.Inventory.Domain.Aggregates;

public class StockItem : AggregateRoot
{
    private readonly List<StockReservation> _reservations = [];

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int Reserved { get; private set; }
    public int Available => Quantity - Reserved;
    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();

    private StockItem() { }

    public static Result<StockItem> Create(Guid productId, int initialQuantity)
    {
        if (productId == Guid.Empty)
            return Result.Failure<StockItem>("ProductId is required");
        if (initialQuantity < 0)
            return Result.Failure<StockItem>("Initial quantity cannot be negative");

        var stockItem = new StockItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = initialQuantity,
            Reserved = 0
        };

        stockItem.RaiseStockChangedEvent();
        return Result.Success(stockItem);
    }

    internal static StockItem Reconstitute(
        Guid id,
        Guid productId,
        int quantity,
        int reserved,
        IEnumerable<StockReservation> reservations)
    {
        var stockItem = new StockItem
        {
            Id = id,
            ProductId = productId,
            Quantity = quantity,
            Reserved = reserved
        };

        stockItem._reservations.AddRange(reservations);
        return stockItem;
    }

    public Result Replenish(int amount)
    {
        if (amount <= 0)
            return Result.Failure("Amount must be positive");

        Quantity += amount;
        RaiseStockChangedEvent();
        return Result.Success();
    }

    public Result SetAvailableQuantity(int availableQuantity)
    {
        if (availableQuantity < 0)
            return Result.Failure("Available quantity cannot be negative");

        Quantity = Reserved + availableQuantity;
        RaiseStockChangedEvent();
        return Result.Success();
    }

    public Result Reserve(Guid orderId, int amount)
    {
        if (orderId == Guid.Empty)
            return Result.Failure("OrderId is required");
        if (amount <= 0)
            return Result.Failure("Amount must be positive");
        if (_reservations.Any(r => r.OrderId == orderId))
            return Result.Failure($"Stock already reserved for order {orderId}");
        if (Available < amount)
            return Result.Failure($"Insufficient stock: available {Available}, requested {amount}");

        var reservationResult = StockReservation.Create(orderId, amount);
        if (reservationResult.IsFailure)
            return Result.Failure(reservationResult.Error);

        _reservations.Add(reservationResult.Value);
        Reserved += amount;
        RaiseStockChangedEvent();

        if (Available == 0)
            RaiseDomainEvent(new StockDepletedEvent(ProductId));

        return Result.Success();
    }

    public Result Release(Guid orderId, int amount)
    {
        if (orderId == Guid.Empty)
            return Result.Failure("OrderId is required");
        if (amount <= 0)
            return Result.Failure("Amount must be positive");

        var reservation = _reservations.SingleOrDefault(r => r.OrderId == orderId);
        if (reservation is null)
            return Result.Failure($"Reservation for order {orderId} was not found");
        if (reservation.Quantity != amount)
            return Result.Failure($"Cannot release {amount}: order {orderId} reserved {reservation.Quantity}");

        _reservations.Remove(reservation);
        Reserved -= amount;
        RaiseStockChangedEvent();
        return Result.Success();
    }

    public Result Deduct(Guid orderId, int amount)
    {
        if (orderId == Guid.Empty)
            return Result.Failure("OrderId is required");
        if (amount <= 0)
            return Result.Failure("Amount must be positive");

        var reservation = _reservations.SingleOrDefault(r => r.OrderId == orderId);
        if (reservation is null)
            return Result.Failure($"Reservation for order {orderId} was not found");
        if (reservation.Quantity != amount)
            return Result.Failure($"Cannot deduct {amount}: order {orderId} reserved {reservation.Quantity}");

        _reservations.Remove(reservation);
        Reserved -= amount;
        Quantity -= amount;
        RaiseStockChangedEvent();
        return Result.Success();
    }

    private void RaiseStockChangedEvent() => RaiseDomainEvent(new StockChangedEvent(ProductId, Available));
}
