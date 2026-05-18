using Microsoft.EntityFrameworkCore;
using Store.EventOutbox.Application.Interfaces;
using Store.EventOutbox.Infrastructure.Entity;
using Store.SharedKernel.Events;

namespace Store.EventOutbox.Infrastructure.Services;

public class EfDomainEventInbox<TDbContext>(TDbContext dbContext) : IDomainEventInbox
    where TDbContext : DbContext
{
    public Task<bool> HasProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken) =>
        dbContext.Set<ProcessedDomainEvent>()
            .AnyAsync(x => x.EventId == eventId && x.Consumer == consumer, cancellationToken);

    public void AddProcessed(IDomainEvent domainEvent, string consumer)
    {
        AddProcessed(domainEvent.EventId, domainEvent.EventType, consumer);
    }

    public void AddProcessed(Guid eventId, string eventType, string consumer)
    {
        dbContext.Set<ProcessedDomainEvent>()
            .Add(ProcessedDomainEvent.Create(eventId, eventType, consumer));
    }
}
