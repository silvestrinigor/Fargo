namespace Fargo.Core.Entities.Articles;

/// <summary>
/// How the stock of an <see cref="Article"/> is tracked; chosen per article and fixed for its
/// lifetime.
/// </summary>
public enum ArticleTrackingMode : byte
{
    /// <summary>
    /// Each physical unit is its own <see cref="Items.Item"/>, so it can be placed, moved and
    /// traced individually. Inventory is the count of item rows. Suited to serialized or
    /// high-value goods.
    /// </summary>
    Entity = 1,

    /// <summary>
    /// Stock is an aggregate quantity recorded through movement events, without one item per unit.
    /// </summary>
    /// <remarks>
    /// Suited to fungible or bulk goods, goods without complete per-unit information, or volumes
    /// too high to keep one item row per unit. Inventory is the net of signed quantities
    /// (received minus dispatched) per container, and can be evaluated at any past instant from
    /// the events' timestamps.
    /// </remarks>
    Event = 2
}
