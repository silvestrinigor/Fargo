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
    /// <param name="at">
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
    /// The chain of items that contained the queried item at <paramref name="at"/>,
    /// ordered by depth. An empty collection when the item had no containment
    /// history covering that instant (for example, it did not yet exist).
    /// </returns>
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
    /// Gets the article inventory of a container as it existed at a specific
    /// instant, reconstructed from the item containment history.
    /// </summary>
    /// <param name="itemContainerGuid">The container item to inspect.</param>
    /// <param name="at">
    /// The instant, in UTC, at which the inventory is evaluated. Containment is
    /// resolved from the history row valid at this instant for each item.
    /// </param>
    /// <param name="articleGuids">
    /// When provided, restricts the inventory to these articles.
    /// </param>
    /// <param name="includeDescendents">
    /// When <see langword="true"/>, counts items nested in descendant containers
    /// (as they were nested at the instant) as well as direct children; otherwise
    /// only items directly inside the container at the instant are counted.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The per-article counts contained in the container at that instant.</returns>
    Task<IReadOnlyCollection<ItemContainerInventoryDto>> GetInventoryInfoByGuidAtInstantAsync(
        Guid itemContainerGuid,
        DateTimeOffset at,
        IReadOnlyCollection<Guid>? articleGuids = null,
        bool includeDescendents = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the event-based inventory contained in a container, computed as the
    /// net of signed movement quantities per event-tracked article.
    /// </summary>
    /// <remarks>
    /// For each event-tracked article, the net quantity is the sum of quantities
    /// moved into the container set minus the sum of quantities moved out of it,
    /// derived from <c>item_moviment_events</c>. When <paramref name="at"/> is
    /// provided, only events that occurred at or before that instant are
    /// considered; otherwise all events are considered. Only non-zero net
    /// quantities are returned. Entity-tracked articles are not included.
    /// </remarks>
    /// <param name="itemContainerGuid">The container item to inspect.</param>
    /// <param name="articleGuids">
    /// When provided, restricts the inventory to these articles.
    /// </param>
    /// <param name="includeDescendents">
    /// When <see langword="true"/>, counts quantities in descendant containers as
    /// well as the container itself.
    /// </param>
    /// <param name="at">
    /// The instant, in UTC, at which the inventory is evaluated. When
    /// <see langword="null"/>, the current inventory is returned.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The per-article net quantities contained in the container.</returns>
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
