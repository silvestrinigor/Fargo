namespace Fargo.Core.Items.Events;

/// <summary>
/// Identifies the kind of event recorded against an event-tracked article.
/// </summary>
public enum ItemEventType : short
{
    /// <summary>
    /// A movement of a quantity between containers, modelled by
    /// <see cref="ItemMovimentEvent"/>.
    /// </summary>
    Moviment = 1,

    /// <summary>
    /// The creation of a unit of stock for an event-tracked article, optionally
    /// placed inside a container.
    /// </summary>
    Creation = 2,

    /// <summary>
    /// The deletion of a previously created unit of stock for an event-tracked
    /// article. A deletion event references the creation event it removes.
    /// </summary>
    Deletion = 3
}
