using Microsoft.EntityFrameworkCore;
using Store.Inventory.Infrastructure.Entity;

namespace Store.Inventory.Infrastructure.DbContexts;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<StockItemEntity> StockItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
