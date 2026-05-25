using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Catalog.Infrastructure.Entity;


namespace Store.Catalog.Infrastructure.Configuration;

public class CatalogConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.HasKey(p => p.ProductId);
        builder.Property(p => p.ProductId).ValueGeneratedNever().IsRequired();
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(64);
        builder.Property(p => p.ProductName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.ProductDescription).IsRequired().HasMaxLength(500);
        builder.Property(p => p.ProductPrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Brand).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Model).IsRequired().HasMaxLength(100);
        builder.Property(p => p.WarrantyMonths).IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Brand);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.ProductPrice);
        builder.HasOne<CategoryEntity>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(p => p.Specifications)
            .WithOne(s => s.Product)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
