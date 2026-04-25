namespace Fargo.Sdk.Items;

/// <summary>Provides CRUD operations for items and routes hub Updated/Deleted events to tracked entities.</summary>
public interface IItemService
{
    /// <summary>Retrieves a single item by its unique identifier.</summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the item does not exist or is not accessible.</exception>
    Task<Item> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of items.</summary>
    /// <param name="articleGuid">Filter to items of this article.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="partitionGuid">Filter to items in this partition.</param>
    /// <param name="noPartition">When <see langword="true"/>, returns only items without a partition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<IReadOnlyCollection<Item>> GetManyAsync(Guid? articleGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, bool? noPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new item for the specified article and returns it as a live entity.</summary>
    /// <param name="articleGuid">The article this item is an instance of.</param>
    /// <param name="firstPartition">An optional initial partition to assign.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<Item> CreateAsync(Guid articleGuid, Guid? firstPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes an item by its unique identifier.</summary>
    /// <param name="itemGuid">The unique identifier of the item to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the item cannot be deleted or is not accessible.</exception>
    Task DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default);
}
