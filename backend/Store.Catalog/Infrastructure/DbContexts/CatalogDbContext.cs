using Microsoft.EntityFrameworkCore;
using Store.EventOutbox.Infrastructure.Extensions;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.DbContexts;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) :  DbContext(options)
{
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<ProductSpecificationEntity> ProductSpecifications { get; set; }
    public DbSet<ProductImageEntity> ProductImages { get; set; }
    public DbSet<ProductAvailabilityEntity> ProductAvailabilities { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        modelBuilder.ConfigureDomainEventOutbox();
    }
}
