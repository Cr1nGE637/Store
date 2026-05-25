using CSharpFunctionalExtensions;

namespace Store.Inventory.Domain.Entities;

public class StockReservation
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public int Quantity { get; private set; }

    private StockReservation() { }

    public static Result<StockReservation> Create(Guid orderId, int quantity)
    {
        if (orderId == Guid.Empty)
            return Result.Failure<StockReservation>("OrderId is required");
        if (quantity <= 0)
            return Result.Failure<StockReservation>("Reservation quantity must be positive");

        return Result.Success(new StockReservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Quantity = quantity
        });
    }

    internal static StockReservation Reconstitute(Guid id, Guid orderId, int quantity) =>
        new()
        {
            Id = id,
            OrderId = orderId,
            Quantity = quantity
        };
}
