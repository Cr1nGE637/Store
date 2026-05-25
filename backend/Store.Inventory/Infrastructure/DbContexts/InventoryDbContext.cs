using Microsoft.EntityFrameworkCore;
using Store.EventOutbox.Infrastructure.Extensions;
using Store.Inventory.Infrastructure.Entity;

namespace Store.Inventory.Infrastructure.DbContexts;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<StockItemEntity> StockItems { get; set; }
    public DbSet<StockReservationEntity> StockReservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        modelBuilder.ConfigureDomainEventOutbox();
    }
}
