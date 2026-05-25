using Store.Catalog.Application.Interfaces;
using Store.Catalog.Infrastructure.DbContexts;
using Store.EventOutbox.Infrastructure.Services;

namespace Store.Catalog.Infrastructure.Services;

public class CatalogDomainEventOutbox(CatalogDbContext dbContext)
    : EfDomainEventOutbox<CatalogDbContext>(dbContext), ICatalogDomainEventOutbox;
