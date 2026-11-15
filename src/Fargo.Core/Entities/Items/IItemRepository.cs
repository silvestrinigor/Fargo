namespace Fargo.Core.Entities.Items;

public interface IItemRepository
{
    Task<Item?> GetByGuidAsync(Guid itemGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the identifiers of the container items nested at any depth inside a container.
    /// </summary>
    /// <remarks>
    /// Non-container descendants are ignored. Returns an empty collection when the item is not a
    /// container. <paramref name="includeRoot"/> adds the item itself to the result.
    /// </remarks>
    Task<IReadOnlyCollection<Guid>> GetContainedDescendantGuidsAsync(
        Guid itemContainerGuid,
        bool includeRoot = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets every item nested at any depth inside a container, of any article type. The container
    /// itself is not included.
    /// </summary>
    Task<IReadOnlyCollection<ContainedItem>> GetContainedItemsAsync(
        Guid itemContainerGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the items of an article placed directly inside a container; items nested in
    /// descendant containers are not counted.
    /// </summary>
    Task<int> CountDirectChildrenByArticleAsync(
        Guid itemContainerGuid,
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <remarks>
    /// Not persisted until the unit of work completes.
    /// </remarks>
    void Add(Item item);

    /// <remarks>
    /// Not persisted until the unit of work completes.
    /// </remarks>
    void Remove(Item item);

    /// <summary>
    /// Acquires one global, transaction-scoped exclusive lock that serializes container-into-container
    /// moves. It must be called inside a transaction and is released when the transaction ends.
    /// </summary>
    /// <remarks>
    /// Validating that a move creates no circular containment reads other items, so two concurrent
    /// moves could each pass and together create a cycle. A cycle can span containers whose paths
    /// only share an interior node, so a per-container key would not serialize the conflicting
    /// moves; one global lock is correct for cycles of any size. Only moving a non-fixed container
    /// item can create a cycle, so the common leaf-item move never takes it.
    /// </remarks>
    Task AcquireContainmentLockAsync(CancellationToken cancellationToken = default);
}
