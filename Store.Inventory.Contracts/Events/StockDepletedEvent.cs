using Store.SharedKernel.Events;

namespace Store.Inventory.Contracts.Events;

public record StockDepletedEvent(Guid ProductId) : IDomainEvent;
