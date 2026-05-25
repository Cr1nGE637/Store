using Store.EventOutbox.Infrastructure.Services;
using Store.Identity.Application.Interfaces;
using Store.Identity.Infrastructure.DbContexts;

namespace Store.Identity.Infrastructure.Services;

public class IdentityDomainEventOutbox(IdentityDbContext dbContext)
    : EfDomainEventOutbox<IdentityDbContext>(dbContext), IIdentityDomainEventOutbox;
