using Store.EventOutbox.Infrastructure.Services;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Infrastructure.DbContexts;

namespace Store.Ordering.Infrastructure.Services;

public class OrderingDomainEventInbox(OrderingDbContext dbContext)
    : EfDomainEventInbox<OrderingDbContext>(dbContext), IOrderingDomainEventInbox;
