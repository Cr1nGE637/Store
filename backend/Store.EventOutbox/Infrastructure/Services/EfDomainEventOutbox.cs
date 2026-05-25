using Microsoft.EntityFrameworkCore;
using Store.EventOutbox.Application.Interfaces;
using Store.EventOutbox.Infrastructure.Entity;
using Store.SharedKernel.Events;

namespace Store.EventOutbox.Infrastructure.Services;

public class EfDomainEventOutbox<TDbContext>(TDbContext dbContext) : IDomainEventOutbox
    where TDbContext : DbContext
{
    public async Task AddAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await dbContext.Set<DomainEventOutboxMessage>()
                .AddAsync(DomainEventOutboxMessage.FromDomainEvent(domainEvent), cancellationToken);
        }
    }
}
