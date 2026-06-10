using Store.SharedKernel.Events;

namespace Store.Consulting.Contracts.Events;

[DomainEventName("consulting.consultation_requested")]
public sealed record ConsultationRequestedEvent(
    Guid ConsultationId,
    Guid? CustomerId,
    IReadOnlyCollection<Guid> ProductIds) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "consulting.consultation_requested";
}
