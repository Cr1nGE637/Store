using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Ordering.Infrastructure.Entity;

namespace Store.Ordering.Infrastructure.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.OrderId);
        builder.Property(o => o.SourceCheckoutId);
        builder.Property(o => o.CustomerId).IsRequired();
        builder.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(256);
        builder.Property(o => o.RecipientName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Phone).IsRequired().HasMaxLength(50);
        builder.Property(o => o.DeliveryAddress).IsRequired().HasMaxLength(500);
        builder.Property(o => o.DeliveryMethod).IsRequired().HasMaxLength(100);
        builder.Property(o => o.PaymentMethod).IsRequired().HasMaxLength(100);
        builder.Property(o => o.Status).IsRequired().HasConversion<string>();
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.PaidAt);
        builder.Property(o => o.CancelledAt);
        builder.Property(o => o.RejectedAt);
        builder.Property(o => o.RejectionReason).HasMaxLength(1024);
        builder.HasMany(o => o.Products)
               .WithOne()
               .HasForeignKey(p => p.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.SourceCheckoutId)
               .IsUnique()
               .HasFilter("\"SourceCheckoutId\" IS NOT NULL");
    }
}
