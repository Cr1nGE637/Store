using Microsoft.EntityFrameworkCore;
using Store.Consulting.Infrastructure.Entity;

namespace Store.Consulting.Infrastructure.DbContexts;

public sealed class ConsultingCatalogReadDbContext(DbContextOptions<ConsultingCatalogReadDbContext> options)
    : DbContext(options)
{
    public DbSet<ConsultingCatalogProductEntity> Products { get; set; }
    public DbSet<ConsultingCatalogCategoryEntity> Categories { get; set; }
    public DbSet<ConsultingCatalogProductSpecificationEntity> ProductSpecifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        modelBuilder.Entity<ConsultingCatalogProductEntity>(builder =>
        {
            builder.ToTable("Products");
            builder.HasKey(product => product.ProductId);
            builder.Property(product => product.ProductName).IsRequired();
            builder.Property(product => product.CategoryId).IsRequired();
            builder.HasOne(product => product.Category)
                .WithMany()
                .HasForeignKey(product => product.CategoryId);
            builder.HasMany(product => product.Specifications)
                .WithOne(specification => specification.Product)
                .HasForeignKey(specification => specification.ProductId);
        });

        modelBuilder.Entity<ConsultingCatalogCategoryEntity>(builder =>
        {
            builder.ToTable("Categories");
            builder.HasKey(category => category.CategoryId);
            builder.Property(category => category.CategoryCode).IsRequired();
        });

        modelBuilder.Entity<ConsultingCatalogProductSpecificationEntity>(builder =>
        {
            builder.ToTable("ProductSpecifications");
            builder.HasKey(specification => new { specification.ProductId, specification.Name });
            builder.Property(specification => specification.Name).IsRequired();
            builder.Property(specification => specification.Value).IsRequired();
        });
    }
}
