namespace Fargo.Domain.Items;

/// <summary>
/// Defines the repository contract for managing <see cref="Item"/> entities.
/// </summary>
public interface IItemRepository
{
    /// <summary>
    /// Gets an item by its unique identifier.
    /// </summary>
    Task<Item?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of item unique identifiers.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetManyGuids(
        Pagination pagination,
        Guid? articleGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of item unique identifiers,
    /// filtered to items that belong to at least one of the specified partitions.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetManyGuidsInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? articleGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new item to the persistence context.
    /// </summary>
    void Add(Item item);

    /// <summary>
    /// Removes an item from the persistence context.
    /// </summary>
    void Remove(Item item);
}
