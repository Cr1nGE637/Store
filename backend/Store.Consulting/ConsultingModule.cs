using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Consulting.Application;
using Store.Consulting.Application.Interfaces;
using Store.Consulting.Application.Services;
using Store.Consulting.Domain.Services;
using Store.Consulting.Infrastructure.DbContexts;
using Store.Consulting.Infrastructure.Repository;

namespace Store.Consulting;

public static class ConsultingModule
{
    public static IServiceCollection AddConsultingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ConsultingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("ConsultingDbConnectionString"),
                npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "consulting");
                    npgsql.EnableRetryOnFailure();
                }));

        services.AddDbContext<ConsultingCatalogReadDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("CatalogDbConnectionString"),
                npgsql => npgsql.EnableRetryOnFailure()));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ConsultingApplicationAssemblyMarker).Assembly));

        services.AddScoped<IConsultingProductReadRepository, ConsultingProductReadRepository>();
        services.AddScoped<CompatibilityRuleRepository>();
        services.AddScoped<ICompatibilityRuleRepository>(sp => sp.GetRequiredService<CompatibilityRuleRepository>());
        services.AddScoped<ICompatibilityRuleProvider>(sp => sp.GetRequiredService<CompatibilityRuleRepository>());
        services.AddScoped<IConsultationResultRepository, ConsultationResultRepository>();
        services.AddSingleton<CompatibilityChecker>();

        return services;
    }
}
