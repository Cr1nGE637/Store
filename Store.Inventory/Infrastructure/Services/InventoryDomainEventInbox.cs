using Store.EventOutbox.Infrastructure.Services;
using Store.Inventory.Application.Interfaces;
using Store.Inventory.Infrastructure.DbContexts;

namespace Store.Inventory.Infrastructure.Services;

public class InventoryDomainEventInbox(InventoryDbContext dbContext)
    : EfDomainEventInbox<InventoryDbContext>(dbContext), IInventoryDomainEventInbox;
