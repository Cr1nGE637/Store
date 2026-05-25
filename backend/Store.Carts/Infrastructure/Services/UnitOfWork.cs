using Store.Carts.Application.Interfaces;
using Store.Carts.Infrastructure.DbContexts;

namespace Store.Carts.Infrastructure.Services;

public class UnitOfWork(CartDbContext context) : ICartUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
