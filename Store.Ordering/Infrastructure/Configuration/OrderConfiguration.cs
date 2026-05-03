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
        builder.Property(o => o.CustomerId).IsRequired();
        builder.Property(o => o.Status).IsRequired().HasConversion<string>();
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.PaidAt);
        builder.Property(o => o.CancelledAt);
        builder.HasMany(o => o.Products)
               .WithOne()
               .HasForeignKey(p => p.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(o => o.CustomerId);
    }
}
