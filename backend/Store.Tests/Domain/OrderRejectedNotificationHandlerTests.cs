using Microsoft.Extensions.Logging.Abstractions;
using Store.Notifications.Application.EventHandlers;
using Store.Notifications.Application.Interfaces;
using Store.Ordering.Contracts.Events;
using Store.SharedKernel.Events;

namespace Store.Tests.Domain;

public class OrderRejectedNotificationHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrderRejected_EnqueuesSingleNotificationWithReason()
    {
        var outbox = new FakeNotificationOutbox();
        var unitOfWork = new FakeNotificationsUnitOfWork();
        var inbox = new FakeNotificationsDomainEventInbox();
        var handler = new OrderRejectedNotificationHandler(
            outbox,
            unitOfWork,
            inbox,
            NullLogger<OrderRejectedNotificationHandler>.Instance);
        var notification = new OrderRejectedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Insufficient stock",
            [new OrderItem(Guid.NewGuid(), "Apple iPhone 15", 79990m, 1)]);

        await handler.Handle(notification, CancellationToken.None);
        await handler.Handle(notification, CancellationToken.None);

        var message = Assert.Single(outbox.Messages);
        Assert.Equal("customer@example.com", message.To);
        Assert.Contains(notification.OrderId.ToString(), message.Subject);
        Assert.Contains(notification.OrderId.ToString(), message.Body);
        Assert.Contains("Insufficient stock", message.Body);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeNotificationOutbox : INotificationOutbox
    {
        public List<NotificationMessage> Messages { get; } = [];

        public Task EnqueueAsync(string to, string subject, string body, string? dedupeKey, CancellationToken cancellationToken)
        {
            if (dedupeKey is not null && Messages.Any(m => m.DedupeKey == dedupeKey))
                return Task.CompletedTask;

            Messages.Add(new NotificationMessage(to, subject, body, dedupeKey));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeNotificationsUnitOfWork : INotificationsUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeNotificationsDomainEventInbox : INotificationsDomainEventInbox
    {
        private readonly HashSet<(Guid EventId, string Consumer)> _processed = [];

        public Task<bool> HasProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken) =>
            Task.FromResult(_processed.Contains((eventId, consumer)));

        public void AddProcessed(IDomainEvent domainEvent, string consumer) =>
            AddProcessed(domainEvent.EventId, domainEvent.EventType, consumer);

        public void AddProcessed(Guid eventId, string eventType, string consumer) =>
            _processed.Add((eventId, consumer));
    }

    private sealed record NotificationMessage(string To, string Subject, string Body, string? DedupeKey);
}
