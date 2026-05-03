using Store.Identity.Application.CQRS.Command;
using Store.Identity.Application.Interfaces;
using Store.Identity.Domain.Interfaces;
using Store.Identity.Infrastructure.Configuration;
using Store.Identity.Infrastructure.DbContexts;
using Store.Identity.Infrastructure.Repository;
using Store.Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Store.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("IdentityDbConnectionString"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "identity")));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IIdentityUnitOfWork, UnitOfWork>();

        return services;
    }
}
