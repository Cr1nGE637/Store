using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Carts.Infrastructure.Entity;

namespace Store.Carts.Infrastructure.Configuration;

public class CheckoutOrderConfiguration : IEntityTypeConfiguration<CheckoutOrderEntity>
{
    public void Configure(EntityTypeBuilder<CheckoutOrderEntity> builder)
    {
        builder.HasKey(o => o.CheckoutId);

        builder.Property(o => o.CheckoutId)
            .ValueGeneratedNever();

        builder.Property(o => o.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(o => o.OrderId)
            .IsUnique();

        builder.HasIndex(o => o.CustomerId);
    }
}
