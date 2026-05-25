using Microsoft.EntityFrameworkCore;
using Store.EventOutbox.Infrastructure.Entity;

namespace Store.EventOutbox.Infrastructure.Extensions;

public static class DomainEventOutboxModelBuilderExtensions
{
    public static ModelBuilder ConfigureDomainEventOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DomainEventOutboxMessage>(builder =>
        {
            builder.ToTable("domain_event_outbox");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.EventId)
                .IsRequired();

            builder.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Version)
                .IsRequired();

            builder.Property(x => x.Payload)
                .IsRequired();

            builder.Property(x => x.OccurredOnUtc)
                .IsRequired();

            builder.Property(x => x.AttemptCount)
                .IsRequired();

            builder.Property(x => x.IsDeadLettered)
                .IsRequired();

            builder.HasIndex(x => new { x.ProcessedOnUtc, x.IsDeadLettered, x.NextAttemptOnUtc });
            builder.HasIndex(x => x.EventId);
        });

        modelBuilder.Entity<ProcessedDomainEvent>(builder =>
        {
            builder.ToTable("processed_domain_events");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.EventId)
                .IsRequired();

            builder.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Consumer)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.ProcessedOnUtc)
                .IsRequired();

            builder.HasIndex(x => new { x.EventId, x.Consumer })
                .IsUnique();
        });

        return modelBuilder;
    }
}
