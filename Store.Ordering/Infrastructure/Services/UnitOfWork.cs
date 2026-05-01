using Store.Ordering.Application.Interfaces;
using Store.Ordering.Infrastructure.DbContexts;

namespace Store.Ordering.Infrastructure.Services;

public class UnitOfWork(OrderingDbContext context) : IOrderingUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
