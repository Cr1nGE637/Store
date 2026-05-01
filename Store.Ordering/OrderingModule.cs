using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Ordering.Application.EventHandlers;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Domain.Interfaces;
using Store.Ordering.Infrastructure.DbContexts;
using Store.Ordering.Infrastructure.Repository;
using Store.Ordering.Infrastructure.Services;

namespace Store.Ordering;

public static class OrderingModule
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("OrderingDbConnectionString"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ordering")));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CartCheckedOutEventHandler).Assembly));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderingUnitOfWork, UnitOfWork>();

        return services;
    }
}
