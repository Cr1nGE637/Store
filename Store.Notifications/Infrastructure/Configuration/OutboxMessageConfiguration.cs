using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Notifications.Infrastructure.Entity;

namespace Store.Notifications.Infrastructure.Configuration;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessageEntity>
{
    public void Configure(EntityTypeBuilder<OutboxMessageEntity> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.To).IsRequired().HasMaxLength(320);
        builder.Property(x => x.Subject).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.DedupeKey).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.AttemptCount).IsRequired();
        builder.Property(x => x.IsDeadLettered).IsRequired();
        builder.HasIndex(x => new { x.ProcessedAt, x.IsDeadLettered, x.NextAttemptAt });
        builder.HasIndex(x => x.DedupeKey)
            .IsUnique()
            .HasFilter("\"DedupeKey\" IS NOT NULL");
    }
}
