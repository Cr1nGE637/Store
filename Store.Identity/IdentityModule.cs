using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.SharedKernel.Interfaces;
using Users.Application.CQRS.Command;
using Users.Application.Interfaces;
using Users.Domain.Interfaces;
using Users.Infrastructure.Configuration;
using Users.Infrastructure.DbContext;
using Users.Infrastructure.Mappers;
using Users.Infrastructure.Repository;
using Users.Infrastructure.Services;

namespace Users;

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

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(UsersProfile).Assembly));

        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
