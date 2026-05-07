using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Store.Catalog.Application.CQRS.Command;

internal static class CatalogPersistenceErrors
{
    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
