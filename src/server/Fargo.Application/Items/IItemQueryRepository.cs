using Fargo.Application.Common;

namespace Fargo.Application.Items;

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
    /// Gets the location hierarchy of an item as it existed at a specific instant,
    /// ordered from the item itself outward to its root container.
    /// </summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="asOf">
    /// The instant, in UTC, at which the containment hierarchy is evaluated. Each
    /// link of the chain is resolved from the containment history valid at this
    /// instant.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// When provided, restricts the result to items accessible through any of
    /// these partitions.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The chain of items that contained the queried item at <paramref name="asOf"/>,
    /// ordered by depth. An empty collection when the item had no containment
    /// history covering that instant (for example, it did not yet exist).
    /// </returns>
    Task<IReadOnlyCollection<ItemDto>> GetLocationInfoByGuidAtInstantOrderedByDepthAsync(
        Guid itemGuid,
        DateTimeOffset asOf,
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
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemContainerInventoryDto>> GetInventoryInfoByGuidAsync(
        IReadOnlyCollection<Guid>? itemContainerGuids = null,
        IReadOnlyCollection<Guid>? articleGuids = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistByGuidAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );
}
