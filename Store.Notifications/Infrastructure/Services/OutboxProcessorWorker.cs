using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Store.Notifications.Application.Interfaces;
using Store.Notifications.Infrastructure.Configuration;
using Store.Notifications.Infrastructure.DbContexts;

namespace Store.Notifications.Infrastructure.Services;

public class OutboxProcessorWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<SmtpOptions> options,
    ILogger<OutboxProcessorWorker> logger) : BackgroundService
{
    private readonly TimeSpan _pollingInterval =
        TimeSpan.FromSeconds(options.Value.OutboxPollingIntervalSeconds);
    private readonly int _maxAttempts = options.Value.OutboxMaxAttempts;
    private readonly int _backoffBaseSeconds = options.Value.OutboxBackoffBaseSeconds;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Outbox processor cycle failed");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<INotificationsUnitOfWork>();

        var now = DateTime.UtcNow;
        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null
                        && !m.IsDeadLettered
                        && (m.NextAttemptAt == null || m.NextAttemptAt <= now))
            .OrderBy(m => m.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        foreach (var msg in messages)
        {
            try
            {
                await sender.SendAsync(msg.To, msg.Subject, msg.Body, ct);
                msg.MarkAsProcessed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email to {To}: {Subject}", msg.To, msg.Subject);
                msg.MarkAsFailed(ex.Message, _maxAttempts, _backoffBaseSeconds);
                if (msg.IsDeadLettered)
                    logger.LogWarning("Outbox message {Id} dead-lettered after {Attempts} attempts", msg.Id, msg.AttemptCount);
            }

            try
            {
                await unitOfWork.SaveChangesAsync(ct);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                logger.LogError(
                    ex,
                    "Failed to persist outbox message {Id} state after attempt {Attempts}. Message may be retried.",
                    msg.Id,
                    msg.AttemptCount);
                db.Entry(msg).State = EntityState.Detached;
            }
        }
    }
}
