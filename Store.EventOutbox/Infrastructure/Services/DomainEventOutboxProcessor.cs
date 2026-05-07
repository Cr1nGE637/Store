using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Store.EventOutbox.Infrastructure.Entity;
using Store.EventOutbox.Infrastructure.Serialization;

namespace Store.EventOutbox.Infrastructure.Services;

public class DomainEventOutboxProcessor<TDbContext>(
    IServiceScopeFactory scopeFactory,
    ILogger<DomainEventOutboxProcessor<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;
    private const int MaxAttempts = 5;
    private const int BackoffBaseSeconds = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Domain event outbox processor failed for {DbContext}", typeof(TDbContext).Name);
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var now = DateTime.UtcNow;

        var messages = await dbContext.Set<DomainEventOutboxMessage>()
            .Where(x => x.ProcessedOnUtc == null &&
                        !x.IsDeadLettered &&
                        (x.NextAttemptOnUtc == null || x.NextAttemptOnUtc <= now))
            .OrderBy(x => x.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var domainEvent = DomainEventSerializer.Deserialize(message.EventType, message.Payload);
                await publisher.Publish(domainEvent, cancellationToken);
                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.Message, MaxAttempts, BackoffBaseSeconds);
                logger.LogError(
                    ex,
                    "Failed to publish domain event outbox message {MessageId} from {DbContext}",
                    message.Id,
                    typeof(TDbContext).Name);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
