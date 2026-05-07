namespace Store.Notifications.Application.Interfaces;

public interface INotificationOutbox
{
    void Enqueue(string to, string subject, string body);
}
