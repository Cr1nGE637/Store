using Store.Inventory.Application.Interfaces;
using Store.Inventory.Infrastructure.DbContexts;

namespace Store.Inventory.Infrastructure.Services;

public class UnitOfWork(InventoryDbContext context) : IInventoryUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
