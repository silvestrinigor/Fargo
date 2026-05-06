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
    /// Retrieves the unique identifiers of all items contained under a given item.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetContainerDescendantGuids(
        Guid itemGuid,
        bool includeRoot = true,
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
