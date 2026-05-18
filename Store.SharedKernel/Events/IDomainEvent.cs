using MediatR;

namespace Store.SharedKernel.Events;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    string EventType { get; }
    int Version { get; }
    DateTime OccurredOnUtc { get; }
    string? CorrelationId { get; }
    string? CausationId { get; }
}
