using Fargo.Sdk.Contracts.Partitions;

namespace Fargo.Sdk.Partitions;

/// <summary>Low-level HTTP transport for partition endpoints.</summary>
public interface IPartitionClient
{
    /// <summary>Retrieves a single partition by its unique identifier.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<PartitionInfo>> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of partitions.</summary>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="childOfAnyOfThesePartitions">Filters to direct children of any of these partitions.</param>
    /// <param name="notChildOfAnyPartition">When <see langword="true"/>, includes root-level partitions.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<IReadOnlyCollection<PartitionInfo>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new partition and returns its assigned identifier.</summary>
    /// <param name="request">The partition creation request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<Guid>> CreateAsync(PartitionCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates the properties of an existing partition.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition to update.</param>
    /// <param name="request">The partition update request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> UpdateAsync(Guid partitionGuid, PartitionUpdateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a partition by its unique identifier.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default);
}
