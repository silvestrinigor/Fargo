using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.Items;

namespace Fargo.Application.Persistence.Repositories;

/// <remarks>
/// For every <c>childOfAnyOfThesePartitions</c> parameter, null applies no partition filter and an
/// empty collection matches nothing.
/// </remarks>
public interface IItemQueryRepository
{
    Task<ItemDto?> GetInfoByGuidAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemDto>> GetLocationInfoByGuidOrderedByDepthAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// The chain of items that contained the item at <paramref name="at"/>, from the item outward to its
    /// root container. Empty when no containment history covers the instant (for example, the item did
    /// not yet exist).
    /// </summary>
    Task<IReadOnlyCollection<ItemDto>> GetLocationInfoByGuidAtInstantOrderedByDepthAsync(
        Guid itemGuid,
        DateTimeOffset at,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemMovimentDto>?> GetItemMovimentsInfoByGuidOrderedByOccurredAtAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<PagedResult<ItemDto>> GetManyInfoOrderedByGuidAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemContainerInventoryDto>> GetInventoryInfoByGuidAsync(
        IReadOnlyCollection<Guid>? itemContainerGuids = null,
        IReadOnlyCollection<Guid>? articleGuids = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Per-article counts in the container as it was at <paramref name="at"/>, reconstructed from the item
    /// containment history.
    /// </summary>
    /// <param name="includeDescendents">
    /// Also count items nested in descendant containers (as nested at the instant); otherwise only direct
    /// children are counted.
    /// </param>
    Task<IReadOnlyCollection<ItemContainerInventoryDto>> GetInventoryInfoByGuidAtInstantAsync(
        Guid itemContainerGuid,
        DateTimeOffset at,
        IReadOnlyCollection<Guid>? articleGuids = null,
        bool includeDescendents = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Net quantity per event-tracked article in the container: quantities moved in minus quantities
    /// moved out, from <c>item_moviment_events</c>.
    /// </summary>
    /// <remarks>
    /// A null <paramref name="at"/> means now; otherwise only events at or before that instant count.
    /// Only non-zero net quantities are returned, and entity-tracked articles are not included.
    /// </remarks>
    Task<IReadOnlyCollection<ItemContainerInventoryDto>> GetEventInventoryInfoByGuidAsync(
        Guid itemContainerGuid,
        IReadOnlyCollection<Guid>? articleGuids = null,
        bool includeDescendents = true,
        DateTimeOffset? at = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistByGuidAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );
}
