using Store.SharedKernel.Events;

namespace Store.EventOutbox.Infrastructure.Entity;

public class ProcessedDomainEvent
{
    private ProcessedDomainEvent() { }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Consumer { get; private set; } = string.Empty;
    public DateTime ProcessedOnUtc { get; private set; }

    public static ProcessedDomainEvent Create(IDomainEvent domainEvent, string consumer) =>
        Create(domainEvent.EventId, domainEvent.EventType, consumer);

    public static ProcessedDomainEvent Create(Guid eventId, string eventType, string consumer) =>
        new()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            EventType = eventType,
            Consumer = consumer,
            ProcessedOnUtc = DateTime.UtcNow
        };
}
