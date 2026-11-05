namespace Fargo.Core.Entities.Partitions;

/// <summary>
/// Defines the repository contract for querying and persisting
/// <see cref="Partition"/> entities.
/// </summary>
/// <remarks>
/// Implementations are responsible for retrieving partitions from the
/// persistence layer and tracking changes for creation and deletion.
/// Changes are typically committed through a unit of work.
/// </remarks>
public interface IPartitionRepository
{
    /// <summary>
    /// Gets a partition by its unique identifier.
    /// </summary>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="Partition"/> if found; otherwise,
    /// <see langword="null"/>.
    /// </returns>
    Task<Partition?> GetByGuidAsync(Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the unique identifiers of all descendant partitions of the
    /// specified partition.
    /// </summary>
    /// <param name="partitionGuid">
    /// The identifier of the root partition.
    /// </param>
    /// <param name="includeRoot">
    /// <see langword="true"/> to include the specified partition in the
    /// result; otherwise, only descendant partitions are returned.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing the identifiers of the matching
    /// descendant partitions.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetDescendantGuidsAsync(
        Guid partitionGuid, bool includeRoot = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the unique identifiers of all descendant partitions of the specified
    /// partitions.
    /// </summary>
    /// <param name="partitionGuids">
    /// The identifiers of the partitions whose descendants should be retrieved.
    /// </param>
    /// <param name="includeRoots">
    /// <see langword="true"/> to include the specified partitions in the result;
    /// otherwise, only descendant partitions are returned.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing the identifiers of the matching
    /// descendant partitions.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetDescendantGuidsAsync(
        IReadOnlyCollection<Guid> partitionGuids, bool includeRoots = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the specified partition has at least one direct child
    /// partition.
    /// </summary>
    /// <param name="partitionGuid">
    /// The identifier of the partition whose children should be checked.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the partition has at least one direct child
    /// partition; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> HasChildrenAsync(Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires a single, global advisory lock that serializes partition
    /// re-parenting across the whole partition tree.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <remarks>
    /// The lock protects the check-then-act in the re-parent flow: validating
    /// that assigning a parent does not create a circular hierarchy reads other
    /// partitions, so two concurrent re-parents could each pass validation and
    /// together create a cycle. A circular hierarchy can span several partitions
    /// whose paths only share an interior node, so a per-partition key would not
    /// reliably serialize the conflicting moves; one global lock serializes every
    /// such re-parent and is therefore correct for cycles of any size. It is only
    /// meant to be taken when actually assigning a parent (the sole change that
    /// can create a cycle). The lock releases automatically when the surrounding
    /// transaction ends, so it must be called inside a transaction.
    /// </remarks>
    Task AcquireHierarchyLockAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new partition to the persistence context.
    /// </summary>
    /// <param name="partition">
    /// The partition to add.
    /// </param>
    /// <remarks>
    /// The partition is tracked by the persistence context. The operation is
    /// not committed until the associated unit of work is completed.
    /// </remarks>
    void Add(Partition partition);

    /// <summary>
    /// Removes a partition from the persistence context.
    /// </summary>
    /// <param name="partition">
    /// The partition to remove.
    /// </param>
    /// <remarks>
    /// The removal is staged in the persistence context and is not committed
    /// until the associated unit of work is completed.
    /// </remarks>
    void Remove(Partition partition);
}
