using Store.Notifications.Application;

namespace Store.Tests.Domain;

public class NotificationTests
{
    [Fact]
    public void NotificationMessageId_Create_UsesStableOutboxMessageId()
    {
        var outboxMessageId = Guid.Parse("11111111-2222-3333-4444-555555555555");

        var messageId = NotificationMessageId.Create(outboxMessageId);

        Assert.Equal("11111111222233334444555555555555@store.local", messageId);
        Assert.Equal(messageId, NotificationMessageId.Create(outboxMessageId));
    }
}
