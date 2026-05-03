using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Inventory.Application.EventHandlers;
using Store.Inventory.Application.Interfaces;
using Store.Inventory.Domain.Interfaces;
using Store.Inventory.Infrastructure.DbContexts;
using Store.Inventory.Infrastructure.Services;
using Store.Inventory.Infrastructure.Repository;

namespace Store.Inventory;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("InventoryDbConnectionString"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "inventory")));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(OrderCreatedEventHandler).Assembly));

        services.AddScoped<IStockItemRepository, StockItemRepository>();
        services.AddScoped<IInventoryUnitOfWork, UnitOfWork>();

        return services;
    }
}
