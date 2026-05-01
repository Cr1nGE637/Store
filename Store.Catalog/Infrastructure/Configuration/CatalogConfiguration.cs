using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Catalog.Infrastructure.Entity;


namespace Store.Catalog.Infrastructure.Configuration;

public class CatalogConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.HasKey(p => p.ProductId);
        builder.Property(p => p.ProductId).ValueGeneratedOnAdd().IsRequired();
        builder.Property(p => p.ProductName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.ProductDescription).IsRequired().HasMaxLength(500);
        builder.Property(p => p.ProductPrice).HasColumnType("decimal(18,2)");
        builder.HasOne<CategoryEntity>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Restrict);
    }
}