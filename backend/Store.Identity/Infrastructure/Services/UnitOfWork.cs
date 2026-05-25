using Microsoft.EntityFrameworkCore;
using Npgsql;
using Store.Identity.Application.Interfaces;
using Store.Identity.Infrastructure.DbContexts;

namespace Store.Identity.Infrastructure.Services;

public class UnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _dbContext;

    public UnitOfWork(IdentityDbContext dbContext)
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
            throw new IdentityUniqueConstraintViolationException(ex);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
