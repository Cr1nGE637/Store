using Store.SharedKernel.Events;

namespace Store.Catalog.Domain.Events;

public record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice) : IDomainEvent;
