namespace Fargo.Core.Articles;

/// <summary>
/// Determines how the stock of an <see cref="Article"/> is tracked.
/// </summary>
/// <remarks>
/// The tracking mode is chosen per article and is fixed for its lifetime. It
/// controls whether physical units are represented individually or only as
/// aggregate quantities.
/// </remarks>
public enum ArticleTrackingMode : byte
{
    /// <summary>
    /// Each physical unit is tracked individually as its own <see cref="Items.Item"/>.
    /// </summary>
    /// <remarks>
    /// Use entity tracking when you control and care about each unit: you can
    /// identify units individually, place and move them one by one, fix them in
    /// place, and reconstruct the location history of a specific unit. Inventory
    /// is the count of item rows. This is the appropriate mode for serialized,
    /// high-value, or otherwise individually managed goods.
    /// </remarks>
    Entity = 1,

    /// <summary>
    /// Stock is tracked as aggregate quantities recorded through a stream of
    /// movement events, without materializing one item per unit.
    /// </summary>
    /// <remarks>
    /// Use event tracking when per-unit control is impractical or unnecessary,
    /// for example:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     the goods are fungible or bulk (screws, liquids, grain) and there is
    ///     no meaningful "single unit" to track;
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     you do not have, or do not want to maintain, complete information for
    ///     each individual unit;
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     the volume of units or the throughput of movements is so high that
    ///     creating and moving one item row per unit would be impractical.
    ///     </description>
    ///   </item>
    /// </list>
    /// Inventory is derived from the events as the net of signed quantities
    /// (received minus dispatched) per container, and can be evaluated at any past
    /// instant from the events' timestamps.
    /// </remarks>
    Event = 2
}
