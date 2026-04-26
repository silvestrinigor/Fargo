namespace Fargo.Domain.Events;

/// <summary>
/// Represents a domain event that occurred on an entity.
/// Events are append-only and immutable after creation.
/// </summary>
public sealed class Event : Entity
{
    /// <summary>Gets the type of event that occurred.</summary>
    public EventType EventType { get; init; }

    /// <summary>Gets the unique identifier of the entity involved in the event.</summary>
    public Guid EntityGuid { get; init; }

    /// <summary>Gets the type of the entity involved in the event.</summary>
    public EntityType EntityType { get; init; }

    /// <summary>Gets the point in time when the event occurred.</summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Gets the unique identifier of the user who triggered the event.</summary>
    public Guid ActorGuid { get; init; }
}
