using Store.SharedKernel.Events;

namespace Store.Identity.Contracts.Events;

public record UserRegisteredEvent(Guid UserId, string Email, string Name) : IDomainEvent;
