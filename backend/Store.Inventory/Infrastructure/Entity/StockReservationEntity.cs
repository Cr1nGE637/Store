namespace Store.Inventory.Infrastructure.Entity;

public class StockReservationEntity
{
    public Guid Id { get; set; }
    public Guid StockItemId { get; set; }
    public Guid OrderId { get; set; }
    public int Quantity { get; set; }

    public StockItemEntity StockItem { get; set; } = null!;
}
