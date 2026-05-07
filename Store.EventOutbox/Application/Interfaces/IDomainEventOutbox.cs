using Store.SharedKernel.Events;

namespace Store.EventOutbox.Application.Interfaces;

public interface IDomainEventOutbox
{
    Task AddAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
