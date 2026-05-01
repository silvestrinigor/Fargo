namespace Fargo.Api.Partitions;

/// <summary>
/// Provides operations for managing partitions.
/// </summary>
/// <remarks>
/// Partitions form a hierarchy that controls access boundaries. Every partitioned entity
/// (articles, items, users, user groups) belongs to one or more partitions.
/// </remarks>
public interface IPartitionClient
{
    /// <summary>
    /// Gets a single partition by its unique identifier.
    /// </summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <param name="temporalAsOf">
    /// When provided, returns the state of the partition as it existed at this point in time.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A response containing the <see cref="PartitionResult"/>, or a <see cref="FargoSdkErrorType.NotFound"/>
    /// error if the partition does not exist or is not accessible.
    /// </returns>
    Task<FargoSdkResponse<PartitionResult>> GetAsync(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of partitions accessible to the current user.
    /// </summary>
    /// <param name="parentPartitionGuid">When provided, returns only direct children of the specified partition.</param>
    /// <param name="temporalAsOf">When provided, returns historical data as of this point in time.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A response containing the collection of partitions. The collection is empty if none are found.</returns>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetManyAsync(
        Guid? parentPartitionGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        bool? rootOnly = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new partition.
    /// </summary>
    /// <param name="name">The display name of the partition.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="parentPartitionGuid">
    /// The parent partition. When <see langword="null"/>, the partition is created under the global root.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A response containing the <see cref="Guid"/> of the newly created partition.</returns>
    Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        Guid? parentPartitionGuid = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing partition. Only the properties provided will be changed.
    /// </summary>
    /// <param name="partitionGuid">The unique identifier of the partition to update.</param>
    /// <param name="description">The new description, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="parentPartitionGuid">
    /// The new parent partition (moves the partition in the hierarchy), or <see langword="null"/> to leave unchanged.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid partitionGuid,
        string? name = null,
        string? description = null,
        Guid? parentPartitionGuid = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a partition. The global root partition cannot be deleted.
    /// </summary>
    /// <param name="partitionGuid">The unique identifier of the partition to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default);
}
