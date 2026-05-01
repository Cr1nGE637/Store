using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Carts.Infrastructure.Entities;

namespace Store.Carts.Infrastructure.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItemEntity>
{
    public void Configure(EntityTypeBuilder<CartItemEntity> builder)
    {
        builder.HasKey(i => i.CartItemId);

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Price)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(i => i.Quantity)
            .IsRequired();
    }
}
