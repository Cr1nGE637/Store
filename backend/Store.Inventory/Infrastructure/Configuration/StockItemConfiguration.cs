using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Inventory.Infrastructure.Entity;

namespace Store.Inventory.Infrastructure.Configuration;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItemEntity>
{
    public void Configure(EntityTypeBuilder<StockItemEntity> builder)
    {
        builder.ToTable("StockItems", table =>
        {
            table.HasCheckConstraint("CK_StockItems_ProductId_NotEmpty", "\"ProductId\" <> '00000000-0000-0000-0000-000000000000'");
            table.HasCheckConstraint("CK_StockItems_Quantity_NonNegative", "\"Quantity\" >= 0");
            table.HasCheckConstraint("CK_StockItems_Reserved_NonNegative", "\"Reserved\" >= 0");
            table.HasCheckConstraint("CK_StockItems_Reserved_NotGreaterThanQuantity", "\"Reserved\" <= \"Quantity\"");
        });

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.HasIndex(s => s.ProductId).IsUnique();
        builder.Property(s => s.Quantity).IsRequired();
        builder.Property(s => s.Reserved).IsRequired();
        builder.Property(s => s.Version).IsRowVersion();
    }
}
