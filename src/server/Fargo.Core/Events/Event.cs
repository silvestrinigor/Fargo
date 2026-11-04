namespace Fargo.Core.Events;

/// <summary>
/// Serves as the abstract base for every event recorded in the system.
/// </summary>
/// <remarks>
/// Each concrete event type (such as <see cref="ItemEvent"/>) derives from this
/// base, which carries the identity (<see cref="Guid"/>) and the event family
/// (<see cref="EventType"/>) common to all events. The concrete event type adds
/// its own kind discriminator and payload.
/// </remarks>
public abstract class Event
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    public Guid Guid { get; protected init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the family the event belongs to.
    /// </summary>
    /// <remarks>
    /// Set once by the derived type.
    /// </remarks>
    public EventType EventType { get; protected init; }

    /// <summary>
    /// Gets the instant, in UTC, at which the event occurred.
    /// </summary>
    /// <remarks>
    /// Defaults to the moment the event is created but may be set by the derived
    /// type to back-date an event (for example, when importing historical data).
    /// This value drives point-in-time reconstruction: evaluations at an instant
    /// consider only events whose <see cref="OccuredAt"/> is at or before it.
    /// </remarks>
    public DateTimeOffset OccuredAt { get; protected init; } = DateTimeOffset.UtcNow;
}
