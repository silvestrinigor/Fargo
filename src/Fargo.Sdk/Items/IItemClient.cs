using Fargo.Sdk.Contracts.Items;
using Fargo.Sdk.Contracts.Partitions;

namespace Fargo.Sdk.Items;

/// <summary>Low-level HTTP transport for item endpoints.</summary>
public interface IItemClient
{
    /// <summary>Retrieves a single item by its unique identifier.</summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<ItemInfo>> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of items.</summary>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="childOfAnyOfThesePartitions">Filters to items that are direct children of any of these partitions.</param>
    /// <param name="notChildOfAnyPartition">When <see langword="true"/>, includes public items without a partition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<IReadOnlyCollection<ItemInfo>>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null, bool? notChildOfAnyPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new item for the specified article and returns its assigned identifier.</summary>
    /// <param name="request">The item creation request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<Guid>> CreateAsync(ItemCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Replaces an existing item (full PUT semantics).</summary>
    /// <param name="itemGuid">The unique identifier of the item to update.</param>
    /// <param name="request">The item update request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> UpdateAsync(
        Guid itemGuid,
        ItemUpdateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an item by its unique identifier.</summary>
    /// <param name="itemGuid">The unique identifier of the item to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default);

    /// <summary>Adds a partition to an item.</summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="partitionGuid">The unique identifier of the partition to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> AddPartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>Removes a partition from an item.</summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="partitionGuid">The unique identifier of the partition to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> RemovePartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>Returns the partitions assigned to an item.</summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<IReadOnlyCollection<PartitionInfo>>> GetPartitionsAsync(Guid itemGuid, CancellationToken cancellationToken = default);
}
