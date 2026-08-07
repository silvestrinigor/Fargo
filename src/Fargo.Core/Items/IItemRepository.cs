namespace Fargo.Core.Items;

/// <summary>
/// Defines the repository contract for querying and persisting
/// <see cref="Item"/> entities.
/// </summary>
/// <remarks>
/// Implementations are responsible for retrieving items from the persistence
/// layer and tracking changes for creation and deletion. Changes are typically
/// committed through a unit of work.
/// </remarks>
public interface IItemRepository
{
    /// <summary>
    /// Gets an item by its unique identifier.
    /// </summary>
    /// <param name="itemGuid">
    /// The unique identifier of the item.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="Item"/> if found; otherwise,
    /// <see langword="null"/>.
    /// </returns>
    Task<Item?> GetByGuidAsync(Guid itemGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the unique identifiers of all descendant container items contained
    /// within the specified item container.
    /// </summary>
    /// <param name="itemContainerGuid">
    /// The identifier of the item container whose descendant container items should
    /// be retrieved.
    /// </param>
    /// <param name="includeRoot">
    /// <see langword="true"/> to include the specified item container in the result;
    /// otherwise, only descendant container items are returned.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing the identifiers of the matching
    /// descendant container items.
    /// </returns>
    /// <remarks>
    /// Only items that are containers are included in the result. Descendant items
    /// that are not containers are ignored.
    ///
    /// If the specified item is not a container, an empty collection is returned
    /// because non-container items cannot contain descendant container items.
    /// </remarks>
    Task<IReadOnlyCollection<Guid>> GetContainedDescendantGuidsAsync(
        Guid itemContainerGuid,
        bool includeRoot = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new item to the persistence context.
    /// </summary>
    /// <param name="item">
    /// The item to add.
    /// </param>
    /// <remarks>
    /// The item is tracked by the persistence context. The operation is not
    /// committed until the associated unit of work is completed.
    /// </remarks>
    void Add(Item item);

    /// <summary>
    /// Removes an item from the persistence context.
    /// </summary>
    /// <param name="item">
    /// The item to remove.
    /// </param>
    /// <remarks>
    /// The removal is staged in the persistence context and is not committed
    /// until the associated unit of work is completed.
    /// </remarks>
    void Remove(Item item);
}
