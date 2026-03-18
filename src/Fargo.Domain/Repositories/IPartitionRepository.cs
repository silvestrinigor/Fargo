using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="Partition"/> entities.
/// </summary>
/// <remarks>
/// This repository provides persistence access and domain queries related to
/// <see cref="Partition"/> aggregates. It exposes both aggregate retrieval methods
/// and lightweight projection queries used for read operations.
/// </remarks>
public interface IPartitionRepository
{
    /// <summary>
    /// Gets a partition by its unique identifier.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the partition.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="Partition"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Partition?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets lightweight information about a partition by its unique identifier.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the partition.</param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned information represents the state of the partition
    /// as it existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="PartitionInformation"/> projection if the partition exists;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    Task<PartitionInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of partition information projections.
    /// </summary>
    /// <param name="pagination">
    /// The pagination configuration used to control the number of returned results
    /// and the starting position of the query.
    /// </param>
    /// <param name="parentPartitionGuid">
    /// Optional filter used to retrieve only partitions that belong to a specific
    /// parent partition. When provided, only child partitions of the specified
    /// parent are returned.
    /// </param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned results represent the state of the partitions
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection of <see cref="PartitionInformation"/> objects.
    /// </returns>
    Task<IReadOnlyCollection<PartitionInformation>> GetManyInfo(
        Pagination pagination,
        Guid? parentPartitionGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the unique identifiers of all descendant partitions of a given partition.
    /// </summary>
    /// <param name="partitionGuid">
    /// The unique identifier of the root partition whose descendants should be retrieved.
    /// </param>
    /// <param name="includeSelf">
    /// Indicates whether the root partition itself should also be included in the result.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing the unique identifiers of all descendant
    /// partitions, optionally including the root partition itself.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetDescendantGuids(
            Guid partitionGuid,
            bool includeSelf = true,
            CancellationToken cancellationToken = default
            );

    /// <summary>
    /// Gets the unique identifiers of all descendant partitions of the specified root partitions.
    /// </summary>
    /// <param name="partitionGuids">
    /// The unique identifiers of the root partitions whose descendants should be retrieved.
    /// </param>
    /// <param name="includeRoots">
    /// Indicates whether the root partitions themselves should also be included in the result.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing the unique identifiers of all descendant
    /// partitions of the specified roots, optionally including the roots themselves.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetDescendantGuids(
            IReadOnlyCollection<Guid> partitionGuids,
            bool includeRoots = true,
            CancellationToken cancellationToken = default
            );

    /// <summary>
    /// Adds a new partition to the persistence context.
    /// </summary>
    /// <param name="partition">The partition to add.</param>
    void Add(Partition partition);

    /// <summary>
    /// Removes a partition from the persistence context.
    /// </summary>
    /// <param name="partition">The partition to remove.</param>
    void Remove(Partition partition);
}
