using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Configuration;

public class OrderedProductConfiguration :  IEntityTypeConfiguration<OrderedProductEntity>
{
    public void Configure(EntityTypeBuilder<OrderedProductEntity> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();
        builder.Property(o => o.ProductId)
            .IsRequired();
        builder.Property(o => o.ProductName)
            .IsRequired();
        builder.Property(o => o.Quantity)
            .IsRequired()
            .HasDefaultValue(1);
        builder.Property(o => o.OrderId)
            .IsRequired();
        builder.Property(o => o.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
        builder.HasOne(o => o.Order)
            .WithMany(o => o.OrderedProducts)
            .HasForeignKey(o => o.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}