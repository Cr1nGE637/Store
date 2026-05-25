using Microsoft.EntityFrameworkCore;
using Npgsql;
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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new CatalogUniqueConstraintViolationException(ex);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
