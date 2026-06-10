using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.EventOutbox.Infrastructure.Services;
using Store.Catalog.Application.CQRS.Command;
using Store.Catalog.Domain.Interfaces;
using Store.Catalog.Infrastructure.Repository;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Application.Services;
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
                npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
                    npgsql.EnableRetryOnFailure();
                }));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.Configure<ProductImageStorageOptions>(configuration.GetSection("ProductImages"));
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IProductImageStorage, LocalProductImageStorage>();
        services.AddScoped<IProductAvailabilityRepository, ProductAvailabilityRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICatalogUnitOfWork, UnitOfWork>();
        services.AddScoped<ICatalogDomainEventOutbox, CatalogDomainEventOutbox>();
        services.AddSingleton<ProductExcelExportBuilder>();
        services.AddSingleton<ProductExcelImportPreviewReader>();
        services.AddHostedService<DomainEventOutboxProcessor<CatalogDbContext>>();

        return services;
    }
}
