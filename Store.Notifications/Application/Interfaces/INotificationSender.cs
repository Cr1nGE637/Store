namespace Store.Notifications.Application.Interfaces;

public interface INotificationSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
