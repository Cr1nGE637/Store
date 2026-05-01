using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Carts.Infrastructure.Entities;

namespace Store.Carts.Infrastructure.Configurations;

public class ProductCacheConfiguration : IEntityTypeConfiguration<ProductCacheEntity>
{
    public void Configure(EntityTypeBuilder<ProductCacheEntity> builder)
    {
        builder.HasKey(p => p.ProductId);

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.IsAvailable)
            .IsRequired();
    }
}
