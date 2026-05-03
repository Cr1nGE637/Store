using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Inventory.Infrastructure.Entity;

namespace Store.Inventory.Infrastructure.Configuration;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItemEntity>
{
    public void Configure(EntityTypeBuilder<StockItemEntity> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.ProductId).IsUnique();
        builder.Property(s => s.Quantity).IsRequired();
        builder.Property(s => s.Reserved).IsRequired();
    }
}
