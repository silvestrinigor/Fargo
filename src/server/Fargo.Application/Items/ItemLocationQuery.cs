using Fargo.Application.Common;

namespace Fargo.Application.Items;

/// <summary>
/// Queries the location hierarchy of an item, optionally as it existed at a
/// specific point in time.
/// </summary>
/// <param name="ItemGuid">
/// The unique identifier of the item for which the location hierarchy is queried.
/// </param>
/// <param name="At">
/// The instant, in UTC, at which the location hierarchy should be evaluated. When
/// <see langword="null"/>, the current location hierarchy is returned. When
/// provided, the hierarchy is reconstructed from the item containment history as
/// it was at that instant.
/// </param>
/// <remarks>
/// The returned collection represents the chain of items containing the queried
/// item. For example, if an item is contained by another item, which is itself
/// contained by another item, the collection contains the queried item and its
/// containing items.
///
/// When <paramref name="At"/> is supplied, each link of the chain is resolved
/// using the containment that was valid at that instant, so the result reflects
/// where the item was located at that time rather than where it is now.
/// </remarks>
public sealed record ItemLocationQuery(
    Guid ItemGuid,
    DateTimeOffset? At = null
) : IQuery<IReadOnlyCollection<ItemDto>>;
