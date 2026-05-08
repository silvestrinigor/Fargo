using Fargo.Sdk.Contracts.Partitions;

namespace Fargo.Sdk.Partitions;

/// <summary>Low-level HTTP transport for partition endpoints.</summary>
public interface IPartitionHttpClient
{
    /// <summary>Retrieves a single partition by its unique identifier.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<PartitionInfo>> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of partitions.</summary>
    /// <param name="parentPartitionGuid">Filter to children of this parent partition.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="rootOnly">When <see langword="true"/>, returns only root-level partitions.</param>
    /// <param name="search">An optional search term to filter by name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, bool? rootOnly = null, string? search = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new partition and returns its assigned identifier.</summary>
    /// <param name="request">The partition creation request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<Guid>> CreateAsync(PartitionCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates the properties of an existing partition.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition to update.</param>
    /// <param name="request">The partition update request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid partitionGuid, PartitionUpdateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a partition by its unique identifier.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default);
}
