namespace Fargo.Domain.Partitions;

/// <summary>
/// Defines the repository contract for managing <see cref="Partition"/> entities.
/// </summary>
public interface IPartitionRepository
{
    /// <summary>
    /// Retrieves a partition by its unique identifier.
    /// </summary>
    Task<Partition?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the unique identifiers of all descendant partitions of a given partition.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetDescendantGuids(
        Guid partitionGuid,
        bool includeRoot = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the unique identifiers of all descendant partitions of the specified root partitions.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetDescendantGuids(
        IReadOnlyCollection<Guid> partitionGuids,
        bool includeRoots = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new partition to the persistence context.
    /// </summary>
    void Add(Partition partition);

    /// <summary>
    /// Removes a partition from the persistence context.
    /// </summary>
    void Remove(Partition partition);
}
