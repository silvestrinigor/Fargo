namespace Fargo.Core.Events;

/// <summary>
/// Identifies the family an <see cref="Event"/> belongs to.
/// </summary>
public enum EventType
{
    /// <summary>
    /// An event recorded against an event-tracked article, modelled by
    /// <see cref="ItemEvent"/>.
    /// </summary>
    Item = 0
}
