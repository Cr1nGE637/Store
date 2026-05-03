using Store.Identity.Application.Interfaces;
using Store.Identity.Infrastructure.DbContexts;

namespace Store.Identity.Infrastructure.Services;

public class UnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext  _dbContext;

    public UnitOfWork(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}