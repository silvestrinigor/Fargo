using Fargo.Domain.Events;

namespace Fargo.Application.Events;

/// <summary>Read-model for a persisted domain event.</summary>
public sealed record EventInformation(
    Guid Guid,
    EventType EventType,
    EntityType EntityType,
    Guid EntityGuid,
    Guid ActorGuid,
    DateTimeOffset OccurredAt
);
