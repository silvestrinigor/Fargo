namespace Fargo.Sdk.Partitions;

/// <summary>
/// Provides high-level operations for managing partitions.
/// Returns live <see cref="Partition"/> entities whose <see cref="Partition.Description"/>
/// setter automatically persists changes to the backend.
/// </summary>
public interface IPartitionManager
{
    /// <summary>Gets a single partition by its unique identifier.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the partition does not exist or is not accessible.</exception>
    Task<Partition> GetAsync(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a paginated list of partitions accessible to the current user.</summary>
    /// <param name="parentPartitionGuid">When provided, returns only direct children of this partition.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<IReadOnlyCollection<Partition>> GetManyAsync(
        Guid? parentPartitionGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new partition and returns it as a live entity.</summary>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<Partition> CreateAsync(
        string name,
        string? description = null,
        Guid? parentPartitionGuid = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a partition.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the partition cannot be deleted or is not accessible.</exception>
    Task DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Raised when any authenticated client creates a partition.</summary>
    event EventHandler<PartitionCreatedEventArgs>? Created;

    /// <summary>Raised when any authenticated client updates a partition.</summary>
    event EventHandler<PartitionUpdatedEventArgs>? Updated;

    /// <summary>Raised when any authenticated client deletes a partition.</summary>
    event EventHandler<PartitionDeletedEventArgs>? Deleted;
}
