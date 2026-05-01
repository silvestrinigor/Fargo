using Fargo.Api.Partitions;

namespace Fargo.Api.Items;

/// <summary>Low-level HTTP transport for item endpoints.</summary>
public interface IItemHttpClient
{
    /// <summary>Retrieves a single item by its unique identifier.</summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<ItemResult>> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of items.</summary>
    /// <param name="articleGuid">Filter to items of this article.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="partitionGuid">Filter to items in this partition.</param>
    /// <param name="noPartition">When <see langword="true"/>, returns only items without a partition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<ItemResult>>> GetManyAsync(Guid? articleGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, bool? noPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new item for the specified article and returns its assigned identifier.</summary>
    /// <param name="articleGuid">The article this item is an instance of.</param>
    /// <param name="firstPartition">An optional initial partition to assign.</param>
    /// <param name="productionDate">An optional production date.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<Guid>> CreateAsync(Guid articleGuid, Guid? firstPartition = null, DateTimeOffset? productionDate = null, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing item.</summary>
    /// <param name="itemGuid">The unique identifier of the item to update.</param>
    /// <param name="productionDate">The new production date, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid itemGuid, DateTimeOffset? productionDate = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes an item by its unique identifier.</summary>
    /// <param name="itemGuid">The unique identifier of the item to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default);

    /// <summary>Adds a partition to an item.</summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="partitionGuid">The unique identifier of the partition to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>Removes a partition from an item.</summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="partitionGuid">The unique identifier of the partition to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>Returns the partitions assigned to an item.</summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(Guid itemGuid, CancellationToken cancellationToken = default);
}
