namespace Fargo.Core.Entities.Partitions;

public interface IPartitionRepository
{
    Task<Partition?> GetByGuidAsync(Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Descendants at any depth. The root itself is included only when
    /// <paramref name="includeRoot"/> is set.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetDescendantGuidsAsync(
        Guid partitionGuid, bool includeRoot = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Descendants at any depth of all the given partitions. The roots themselves are included only
    /// when <paramref name="includeRoots"/> is set.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetDescendantGuidsAsync(
        IReadOnlyCollection<Guid> partitionGuids, bool includeRoots = true, CancellationToken cancellationToken = default);

    Task<bool> HasChildrenAsync(Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires a single global advisory lock that serializes partition re-parenting across the
    /// whole tree.
    /// </summary>
    /// <remarks>
    /// Validating that a parent assignment creates no cycle reads other partitions, so two concurrent
    /// re-parents could each pass and together create one. A cycle can span partitions that only
    /// share an interior node, so a per-partition key would not serialize them; one global lock
    /// does. Take it only when assigning a parent, the sole change that can create a cycle. It is
    /// released when the surrounding transaction ends, so call it inside a transaction.
    /// </remarks>
    Task AcquireHierarchyLockAsync(CancellationToken cancellationToken = default);

    void Add(Partition partition);

    void Remove(Partition partition);
}
