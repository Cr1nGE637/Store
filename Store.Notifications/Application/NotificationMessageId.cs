namespace Store.Notifications.Application;

public static class NotificationMessageId
{
    public static string Create(Guid outboxMessageId) => $"{outboxMessageId:N}@store.local";
}
