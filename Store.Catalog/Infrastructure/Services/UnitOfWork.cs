using Store.Catalog.Application.Interfaces;
using Store.Catalog.Infrastructure.DbContexts;

namespace Store.Catalog.Infrastructure.Services;

public class UnitOfWork : ICatalogUnitOfWork
{
    private readonly CatalogDbContext _dbContext;

    public UnitOfWork(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
