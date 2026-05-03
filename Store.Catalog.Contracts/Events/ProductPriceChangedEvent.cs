using Store.SharedKernel.Events;

namespace Store.Catalog.Contracts.Events;

public record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice) : IDomainEvent;
