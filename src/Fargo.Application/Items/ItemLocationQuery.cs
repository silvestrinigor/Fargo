using Fargo.Application.Common;

namespace Fargo.Application.Items;

/// <summary>
/// Queries the location hierarchy of an item.
/// </summary>
/// <param name="ItemGuid">
/// The unique identifier of the item for which the location hierarchy is queried.
/// </param>
/// <remarks>
/// The returned collection represents the chain of items containing the queried item.
/// For example, if an item is contained by another item, which is itself contained
/// by another item, the collection contains the queried item and its containing
/// items.
/// </remarks>
public sealed record ItemLocationQuery(
    Guid ItemGuid
) : IQuery<IReadOnlyCollection<ItemDto>>;
