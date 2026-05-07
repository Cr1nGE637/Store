namespace Store.Notifications.Infrastructure.Entity;

public class OutboxMessageEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string To { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? NextAttemptAt { get; private set; }
    public bool IsDeadLettered { get; private set; }

    private const int MaxBackoffDelaySeconds = 60 * 60;

    private OutboxMessageEntity() { }

    public static OutboxMessageEntity Create(string to, string subject, string body) =>
        new() { To = to, Subject = subject, Body = body };

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Error = null;
        NextAttemptAt = null;
    }

    public void MarkAsFailed(string error, int maxAttempts, int backoffBaseSeconds)
    {
        AttemptCount++;
        Error = error;

        if (AttemptCount >= maxAttempts)
        {
            IsDeadLettered = true;
            NextAttemptAt = null;
            return;
        }

        var normalizedBaseSeconds = Math.Max(1, backoffBaseSeconds);
        var exponent = Math.Min(AttemptCount - 1, 30);
        var delaySeconds = Math.Min(
            MaxBackoffDelaySeconds,
            normalizedBaseSeconds * (long)Math.Pow(2, exponent));
        NextAttemptAt = DateTime.UtcNow.AddSeconds(delaySeconds);
    }
}
