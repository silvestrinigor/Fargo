namespace Fargo.Api.Partitions;

/// <summary>Provides CRUD operations for partitions and routes hub Updated/Deleted events to tracked entities.</summary>
public interface IPartitionService
{
    /// <summary>Retrieves a single partition by its unique identifier.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the partition does not exist or is not accessible.</exception>
    Task<Partition> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of partitions.</summary>
    /// <param name="parentPartitionGuid">Filter to children of this parent partition.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="rootOnly">When <see langword="true"/>, returns only root-level partitions.</param>
    /// <param name="search">An optional search term to filter by name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<IReadOnlyCollection<Partition>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, bool? rootOnly = null, string? search = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new partition and returns it as a live entity.</summary>
    /// <param name="name">The partition name.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="parentPartitionGuid">An optional parent partition identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<Partition> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes a partition by its unique identifier.</summary>
    /// <param name="partitionGuid">The unique identifier of the partition to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the partition cannot be deleted or is not accessible.</exception>
    Task DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default);
}
