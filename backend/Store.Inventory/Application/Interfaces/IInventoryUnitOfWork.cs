namespace Store.Inventory.Application.Interfaces;

public interface IInventoryUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
