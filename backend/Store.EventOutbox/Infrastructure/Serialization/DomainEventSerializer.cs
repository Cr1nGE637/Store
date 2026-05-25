using System.Reflection;
using System.Text.Json;
using Store.SharedKernel.Events;

namespace Store.EventOutbox.Infrastructure.Serialization;

public static class DomainEventSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> EventTypes = new(BuildEventTypeMap);

    public static (string EventType, string Payload) Serialize(IDomainEvent domainEvent)
    {
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), Options);
        return (domainEvent.EventType, payload);
    }

    public static IDomainEvent Deserialize(string eventType, string payload)
    {
        var type = ResolveEventType(eventType)
            ?? throw new InvalidOperationException($"Event type '{eventType}' cannot be resolved");

        var domainEvent = JsonSerializer.Deserialize(payload, type, Options);
        if (domainEvent is not IDomainEvent typedEvent)
            throw new InvalidOperationException($"Event type '{eventType}' does not implement IDomainEvent");

        return typedEvent;
    }

    private static Type? ResolveEventType(string eventType) =>
        EventTypes.Value.GetValueOrDefault(eventType) ?? Type.GetType(eventType);

    private static IReadOnlyDictionary<string, Type> BuildEventTypeMap()
    {
        var eventTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes().AsEnumerable();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(type => type is not null).Cast<Type>();
                }
            })
            .Where(type => type is { IsAbstract: false } && typeof(IDomainEvent).IsAssignableFrom(type))
            .Select(type => new
            {
                Type = type,
                Name = type.GetCustomAttributes(typeof(DomainEventNameAttribute), false)
                    .OfType<DomainEventNameAttribute>()
                    .SingleOrDefault()
                    ?.Name
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .ToDictionary(x => x.Name!, x => x.Type);

        return eventTypes;
    }
}
