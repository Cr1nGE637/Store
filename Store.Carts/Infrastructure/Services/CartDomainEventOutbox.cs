using Store.Carts.Application.Interfaces;
using Store.Carts.Infrastructure.DbContexts;
using Store.EventOutbox.Infrastructure.Services;

namespace Store.Carts.Infrastructure.Services;

public class CartDomainEventOutbox(CartDbContext dbContext)
    : EfDomainEventOutbox<CartDbContext>(dbContext), ICartDomainEventOutbox;
