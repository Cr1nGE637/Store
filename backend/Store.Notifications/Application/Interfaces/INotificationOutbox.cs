namespace Store.Notifications.Application.Interfaces;

public interface INotificationOutbox
{
    Task EnqueueAsync(string to, string subject, string body, string? dedupeKey, CancellationToken cancellationToken);
}
