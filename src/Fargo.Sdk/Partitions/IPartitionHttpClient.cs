namespace Fargo.Sdk.Partitions;

/// <summary>Low-level HTTP transport for partition endpoints.</summary>
public interface IPartitionHttpClient
{
    /// <summary>Retrieves a single partition by its unique identifier.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<PartitionResult>> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of partitions.</summary>
    /// <param name="parentPartitionGuid">Filter to children of this parent partition.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="rootOnly">When <see langword="true"/>, returns only root-level partitions.</param>
    /// <param name="search">An optional search term to filter by name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, bool? rootOnly = null, string? search = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new partition and returns its assigned identifier.</summary>
    /// <param name="name">The partition name.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="parentPartitionGuid">An optional parent partition identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<Guid>> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken cancellationToken = default);

    /// <summary>Updates the properties of an existing partition.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition to update.</param>
    /// <param name="name">New name, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="description">New description, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="parentPartitionGuid">New parent partition, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="isActive">New active state, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid partitionGuid, string? name = null, string? description = null, Guid? parentPartitionGuid = null, bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes a partition by its unique identifier.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default);
}
