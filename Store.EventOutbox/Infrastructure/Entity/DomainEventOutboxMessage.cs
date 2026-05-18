using Store.SharedKernel.Events;

using Store.EventOutbox.Infrastructure.Serialization;

namespace Store.EventOutbox.Infrastructure.Entity;

public class DomainEventOutboxMessage
{
    private DomainEventOutboxMessage() { }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public int Version { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? NextAttemptOnUtc { get; private set; }
    public bool IsDeadLettered { get; private set; }
    public string? Error { get; private set; }

    public static DomainEventOutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        var (eventType, payload) = DomainEventSerializer.Serialize(domainEvent);

        return new DomainEventOutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = domainEvent.EventId,
            EventType = eventType,
            Version = domainEvent.Version,
            Payload = payload,
            OccurredOnUtc = domainEvent.OccurredOnUtc
        };
    }

    public void MarkProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error, int maxAttempts, int backoffBaseSeconds)
    {
        AttemptCount++;
        Error = error;

        if (AttemptCount >= maxAttempts)
        {
            IsDeadLettered = true;
            NextAttemptOnUtc = null;
            return;
        }

        var delaySeconds = Math.Pow(backoffBaseSeconds, AttemptCount);
        NextAttemptOnUtc = DateTime.UtcNow.AddSeconds(delaySeconds);
    }
}
