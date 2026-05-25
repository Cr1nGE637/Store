using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Inventory.Infrastructure.Entity;

namespace Store.Inventory.Infrastructure.Configuration;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservationEntity>
{
    public void Configure(EntityTypeBuilder<StockReservationEntity> builder)
    {
        builder.ToTable("StockReservations", table =>
        {
            table.HasCheckConstraint("CK_StockReservations_OrderId_NotEmpty", "\"OrderId\" <> '00000000-0000-0000-0000-000000000000'");
            table.HasCheckConstraint("CK_StockReservations_Quantity_Positive", "\"Quantity\" > 0");
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.StockItemId).IsRequired();
        builder.Property(r => r.OrderId).IsRequired();
        builder.Property(r => r.Quantity).IsRequired();

        builder.HasIndex(r => new { r.StockItemId, r.OrderId }).IsUnique();

        builder.HasOne(r => r.StockItem)
            .WithMany(s => s.Reservations)
            .HasForeignKey(r => r.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
