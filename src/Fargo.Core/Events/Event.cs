namespace Fargo.Core.Events;

public abstract class Event
{
    public Guid Guid { get; protected init; } = Guid.NewGuid();

    public EventType EventType { get; protected init; }

    /// <summary>
    /// UTC instant at which the event occurred. Defaults to creation time but can be back-dated (for
    /// example when importing history). Point-in-time evaluations consider only events at or before
    /// the evaluated instant.
    /// </summary>
    public DateTimeOffset OccuredAt { get; protected init; } = DateTimeOffset.UtcNow;
}
