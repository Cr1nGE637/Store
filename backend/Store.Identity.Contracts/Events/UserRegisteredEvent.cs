using Store.SharedKernel.Events;

namespace Store.Identity.Contracts.Events;

[DomainEventName(EventTypeName)]
public record UserRegisteredEvent(Guid UserId, string Email, string Name) : DomainEvent(EventTypeName)
{
    public const string EventTypeName = "identity.user_registered";
}
