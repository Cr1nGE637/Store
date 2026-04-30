using Microsoft.EntityFrameworkCore;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.DbContext;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) :  Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<ProductEntity> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}