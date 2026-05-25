using Microsoft.EntityFrameworkCore;
using Npgsql;
using Store.Carts.Infrastructure.DbContexts;
using Store.Catalog.Infrastructure.DbContexts;
using Store.Identity.Infrastructure.DbContexts;
using Store.Inventory.Infrastructure.DbContexts;
using Store.Notifications.Infrastructure.DbContexts;
using Store.Ordering.Infrastructure.DbContexts;

namespace Store.Tests.Infrastructure;

public sealed class MigrationSmokeTests
{
    private const string AdminConnectionStringVariable = "STORE_MIGRATION_SMOKE_ADMIN_CONNECTION";

    [Fact]
    [Trait("Category", "Integration")]
    public async Task All_module_migrations_create_expected_schemas_and_tables()
    {
        var adminConnectionString = Environment.GetEnvironmentVariable(AdminConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(adminConnectionString))
        {
            return;
        }

        var databaseName = $"store_migration_smoke_{Guid.NewGuid():N}";
        var databaseConnectionString = await CreateDatabaseAsync(adminConnectionString, databaseName);

        try
        {
            await MigrateAllContextsAsync(databaseConnectionString);
            await AssertExpectedTablesAsync(databaseConnectionString);
        }
        finally
        {
            await DropDatabaseAsync(adminConnectionString, databaseName);
        }
    }

    private static async Task<string> CreateDatabaseAsync(string adminConnectionString, string databaseName)
    {
        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"""CREATE DATABASE "{databaseName}";""";
        await command.ExecuteNonQueryAsync();

        var builder = new NpgsqlConnectionStringBuilder(adminConnectionString)
        {
            Database = databaseName
        };

        return builder.ConnectionString;
    }

    private static async Task MigrateAllContextsAsync(string databaseConnectionString)
    {
        await using var identity = new IdentityDbContext(
            new DbContextOptionsBuilder<IdentityDbContext>()
                .UseNpgsql(
                    WithSearchPath(databaseConnectionString, "identity"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
                .Options);
        await identity.Database.MigrateAsync();

        await using var catalog = new CatalogDbContext(
            new DbContextOptionsBuilder<CatalogDbContext>()
                .UseNpgsql(
                    WithSearchPath(databaseConnectionString, "catalog"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"))
                .Options);
        await catalog.Database.MigrateAsync();

        await using var cart = new CartDbContext(
            new DbContextOptionsBuilder<CartDbContext>()
                .UseNpgsql(
                    WithSearchPath(databaseConnectionString, "cart"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "cart"))
                .Options);
        await cart.Database.MigrateAsync();

        await using var ordering = new OrderingDbContext(
            new DbContextOptionsBuilder<OrderingDbContext>()
                .UseNpgsql(
                    WithSearchPath(databaseConnectionString, "ordering"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ordering"))
                .Options);
        await ordering.Database.MigrateAsync();

        await using var inventory = new InventoryDbContext(
            new DbContextOptionsBuilder<InventoryDbContext>()
                .UseNpgsql(
                    WithSearchPath(databaseConnectionString, "inventory"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "inventory"))
                .Options);
        await inventory.Database.MigrateAsync();

        await using var notifications = new NotificationsDbContext(
            new DbContextOptionsBuilder<NotificationsDbContext>()
                .UseNpgsql(
                    WithSearchPath(databaseConnectionString, "notifications"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "notifications"))
                .Options);
        await notifications.Database.MigrateAsync();
    }

    private static string WithSearchPath(string connectionString, string schema)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            SearchPath = schema
        };

        return builder.ConnectionString;
    }

    private static async Task AssertExpectedTablesAsync(string databaseConnectionString)
    {
        await using var connection = new NpgsqlConnection(databaseConnectionString);
        await connection.OpenAsync();

        await AssertTableExistsAsync(connection, "identity", "Users");
        await AssertTableExistsAsync(connection, "catalog", "Categories");
        await AssertTableExistsAsync(connection, "catalog", "Products");
        await AssertTableExistsAsync(connection, "cart", "Carts");
        await AssertTableExistsAsync(connection, "cart", "CartItems");
        await AssertTableExistsAsync(connection, "cart", "ProductCache");
        await AssertTableExistsAsync(connection, "ordering", "Orders");
        await AssertTableExistsAsync(connection, "ordering", "OrderedProducts");
        await AssertTableExistsAsync(connection, "ordering", "processed_domain_events");
        await AssertTableExistsAsync(connection, "inventory", "StockItems");
        await AssertTableExistsAsync(connection, "inventory", "StockReservations");
        await AssertTableExistsAsync(connection, "inventory", "processed_domain_events");
        await AssertTableExistsAsync(connection, "notifications", "outbox_messages");
        await AssertTableExistsAsync(connection, "notifications", "processed_domain_events");
        await AssertTableMissingAsync(connection, "public", "StockItems");
        await AssertTableMissingAsync(connection, "public", "outbox_messages");
    }

    private static async Task AssertTableExistsAsync(NpgsqlConnection connection, string schema, string table)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)::int
            FROM information_schema.tables
            WHERE table_schema = @schema AND table_name = @table;
            """;
        command.Parameters.AddWithValue("schema", schema);
        command.Parameters.AddWithValue("table", table);

        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        Assert.Equal(1, count);
    }

    private static async Task AssertTableMissingAsync(NpgsqlConnection connection, string schema, string table)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)::int
            FROM information_schema.tables
            WHERE table_schema = @schema AND table_name = @table;
            """;
        command.Parameters.AddWithValue("schema", schema);
        command.Parameters.AddWithValue("table", table);

        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        Assert.Equal(0, count);
    }

    private static async Task DropDatabaseAsync(string adminConnectionString, string databaseName)
    {
        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        await using var terminateCommand = connection.CreateCommand();
        terminateCommand.CommandText =
            $"""
             SELECT pg_terminate_backend(pid)
             FROM pg_stat_activity
             WHERE datname = '{databaseName}';
             """;
        await terminateCommand.ExecuteNonQueryAsync();

        await using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText =
            $"""
             DROP DATABASE IF EXISTS "{databaseName}";
             """;
        await dropCommand.ExecuteNonQueryAsync();
    }
}
