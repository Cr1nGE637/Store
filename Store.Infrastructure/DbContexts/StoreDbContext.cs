using Microsoft.EntityFrameworkCore;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.DbContexts;

public class StoreDbContext(DbContextOptions<StoreDbContext> options) :  DbContext(options)
{
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<CustomerEntity> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreDbContext).Assembly);
    }
}