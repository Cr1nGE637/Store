namespace Store.Inventory.Infrastructure.Entity;

public class StockItemEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int Reserved { get; set; }
    public uint Version { get; set; }
    public List<StockReservationEntity> Reservations { get; set; } = [];
}
