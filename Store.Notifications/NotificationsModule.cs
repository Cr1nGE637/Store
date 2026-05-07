using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.EventOutbox.Infrastructure.Services;
using Store.Notifications.Application.EventHandlers;
using Store.Notifications.Application.Interfaces;
using Store.Notifications.Infrastructure.Configuration;
using Store.Notifications.Infrastructure.DbContexts;
using Store.Notifications.Infrastructure.Services;

namespace Store.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection("SmtpOptions"));

        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("NotificationsDbConnectionString"),
                npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "notifications");
                    npgsql.EnableRetryOnFailure();
                }));

        services.AddScoped<INotificationSender, EmailNotificationSender>();
        services.AddScoped<INotificationOutbox, NotificationOutbox>();
        services.AddScoped<INotificationsUnitOfWork, UnitOfWork>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(OrderCreatedNotificationHandler).Assembly));

        services.AddHostedService<OutboxProcessorWorker>();
        services.AddHostedService<DomainEventOutboxProcessor<NotificationsDbContext>>();

        return services;
    }
}
