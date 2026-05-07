using System.Text.Json;
using Store.SharedKernel.Events;

namespace Store.EventOutbox.Infrastructure.Serialization;

public static class DomainEventSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static (string EventType, string Payload) Serialize(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType().AssemblyQualifiedName
            ?? throw new InvalidOperationException($"Event type name is not available for {domainEvent.GetType().Name}");

        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), Options);
        return (eventType, payload);
    }

    public static IDomainEvent Deserialize(string eventType, string payload)
    {
        var type = Type.GetType(eventType)
            ?? throw new InvalidOperationException($"Event type '{eventType}' cannot be resolved");

        var domainEvent = JsonSerializer.Deserialize(payload, type, Options);
        if (domainEvent is not IDomainEvent typedEvent)
            throw new InvalidOperationException($"Event type '{eventType}' does not implement IDomainEvent");

        return typedEvent;
    }
}
