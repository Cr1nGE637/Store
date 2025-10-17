using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Configuration;

public class OrdersConfiguration :  IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(o => o.TotalPrice)
            .HasColumnType("decimal(18,2)");
        
        builder.HasOne(c => c.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}