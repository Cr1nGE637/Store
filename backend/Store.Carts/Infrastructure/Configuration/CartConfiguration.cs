using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Carts.Infrastructure.Entity;

namespace Store.Carts.Infrastructure.Configuration;

public class CartConfiguration : IEntityTypeConfiguration<CartEntity>
{
    public void Configure(EntityTypeBuilder<CartEntity> builder)
    {
        builder.HasKey(c => c.CartId);

        builder.Property(c => c.CartId)
            .ValueGeneratedNever();

        builder.Property(c => c.IsCheckoutPending)
            .IsRequired();

        builder.Property(c => c.PendingCheckoutId);

        builder.Property(c => c.CheckoutPendingSince);

        builder.HasIndex(c => c.CustomerId).IsUnique();

        builder.HasIndex(c => c.CheckoutPendingSince);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
