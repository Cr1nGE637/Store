using Identity.Application.Interfaces;
using Identity.Infrastructure.DbContexts;

namespace Identity.Infrastructure.Services;

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