namespace Fargo.Sdk.Contracts.Events;

/// <summary>Represents a persisted domain event returned by the API.</summary>
public sealed record EventInfo(
    Guid Guid,
    EventType EventType,
    EntityType EntityType,
    Guid EntityGuid,
    Guid ActorGuid,
    DateTimeOffset OccurredAt);
