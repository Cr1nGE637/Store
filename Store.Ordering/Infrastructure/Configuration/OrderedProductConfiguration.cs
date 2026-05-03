using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Ordering.Infrastructure.Entity;

namespace Store.Ordering.Infrastructure.Configuration;

public class OrderedProductConfiguration : IEntityTypeConfiguration<OrderedProductEntity>
{
    public void Configure(EntityTypeBuilder<OrderedProductEntity> builder)
    {
        builder.ToTable("OrderedProducts");
        builder.HasKey(p => p.OrderedProductId);
        builder.Property(p => p.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(p => p.Quantity).IsRequired();
    }
}
