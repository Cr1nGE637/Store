using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.Configuration;

public class ProductAvailabilityConfiguration : IEntityTypeConfiguration<ProductAvailabilityEntity>
{
    public void Configure(EntityTypeBuilder<ProductAvailabilityEntity> builder)
    {
        builder.ToTable("ProductAvailabilities");
        builder.HasKey(a => a.ProductId);
        builder.Property(a => a.ProductId).ValueGeneratedNever();
        builder.Property(a => a.AvailableQuantity).IsRequired();
        builder.Property(a => a.UpdatedOnUtc).IsRequired();
        builder.HasIndex(a => a.AvailableQuantity);
    }
}
