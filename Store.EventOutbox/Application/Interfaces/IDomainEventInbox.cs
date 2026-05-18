using Store.SharedKernel.Events;

namespace Store.EventOutbox.Application.Interfaces;

public interface IDomainEventInbox
{
    Task<bool> HasProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken);
    void AddProcessed(IDomainEvent domainEvent, string consumer);
    void AddProcessed(Guid eventId, string eventType, string consumer);
}
