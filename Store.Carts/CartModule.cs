using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Carts.Application.CQRS.Command;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;
using Store.Carts.Infrastructure.DbContexts;
using Store.Carts.Infrastructure.Repository;
using Store.Carts.Infrastructure.Services;

namespace Store.Carts;

public static class CartModule
{
    public static IServiceCollection AddCartModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CartDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("CartDbConnectionString"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "cart")));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AddItemCommand).Assembly));

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IProductCacheRepository, ProductCacheRepository>();
        services.AddScoped<ICartUnitOfWork, UnitOfWork>();

        return services;
    }
}
