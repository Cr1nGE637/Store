using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Store.App.Extensions;

public static class MigrationExtension
{
    public static async Task ApplyMigrationsAsync<TContext>(this IServiceProvider provider)
        where TContext : DbContext
    {
        var context = provider.GetRequiredService<TContext>();
        var logger = provider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Store.App.Migrations");

        try
        {
            logger.LogInformation("Applying migrations for {DbContext}", typeof(TContext).Name);
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied for {DbContext}", typeof(TContext).Name);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to apply migrations for {DbContext}", typeof(TContext).Name);
            throw;
        }
    }
}