namespace Store.Inventory.Application.Interfaces;

public interface IInventoryUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
