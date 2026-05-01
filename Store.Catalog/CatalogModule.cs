using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Catalog.Application.CQRS.Command;
using Store.Catalog.Domain.Interfaces;
using Store.Catalog.Infrastructure.Repository;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Infrastructure.DbContexts;
using Store.Catalog.Infrastructure.Services;

namespace Store.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("CatalogDbConnectionString"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog")));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICatalogUnitOfWork, UnitOfWork>();

        return services;
    }
}
