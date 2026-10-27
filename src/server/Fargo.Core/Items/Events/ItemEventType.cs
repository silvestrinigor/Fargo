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
    Moviment = 1
}
