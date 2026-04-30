using Store.SharedKernel.Interfaces;
using Users.Infrastructure.DbContext;

namespace Users.Infrastructure.Services;

public class UnitOfWork  : IUnitOfWork
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