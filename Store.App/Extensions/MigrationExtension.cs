using Microsoft.EntityFrameworkCore;

namespace Store.App.Extensions;

public static class MigrationExtension
{
    public static async Task ApplyMigrationsAsync<TContext>(this IServiceProvider provider)
        where TContext : DbContext
    {
        var context = provider.GetRequiredService<TContext>();

        Console.WriteLine($"Applying migrations for {typeof(TContext).Name}...");
        await context.Database.MigrateAsync();
        Console.WriteLine($"Migrations applied for {typeof(TContext).Name}");
    }
}