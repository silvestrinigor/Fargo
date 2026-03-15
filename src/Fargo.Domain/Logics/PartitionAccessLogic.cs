namespace Fargo.Domain.Logics;

/// <summary>
/// Represents a partition in the system.
/// </summary>
public interface IPartition;

/// <summary>
/// Represents an entity that has access to one or more partitions.
/// </summary>
public interface IPartitionUser
{
    /// <summary>
    /// Gets the collection of partition accesses granted to the user.
    /// </summary>
    IReadOnlyCollection<IPartitionAccess> PartitionAccesses { get; }
}

/// <summary>
/// Represents a user's access to a specific partition.
/// </summary>
public interface IPartitionAccess
{
    /// <summary>
    /// Gets the partition associated with this access.
    /// </summary>
    IPartition Partition { get; }
}

/// <summary>
/// Represents an entity that is associated with one or more partitions.
/// </summary>
public interface IPartitioned
{
    /// <summary>
    /// Gets the partitions associated with the entity.
    /// </summary>
    IReadOnlyCollection<IPartition> Partitions { get; }
}

/// <summary>
/// Provides extension methods for evaluating partition-based access rules.
/// </summary>
public static class PartitionAccessLogic
{
    /// <summary>
    /// Determines whether a user has access to a partitioned entity.
    /// </summary>
    public static bool HasAccess(this IPartitioned partitioned, IPartitionUser partitionUser)
    {
        ArgumentNullException.ThrowIfNull(partitioned);
        ArgumentNullException.ThrowIfNull(partitionUser);

        if (partitioned.Partitions.Count == 0)
        {
            return true;
        }

        return partitioned.Partitions.Any(partition =>
            partitionUser.PartitionAccesses.Any(access => access.Partition == partition));
    }

    /// <summary>
    /// Determines whether a user has access to a specific partition.
    /// </summary>
    public static bool HasAccess(this IPartition partition, IPartitionUser partitionUser)
    {
        ArgumentNullException.ThrowIfNull(partition);
        ArgumentNullException.ThrowIfNull(partitionUser);

        return partitionUser.PartitionAccesses
            .Any(access => access.Partition == partition);
    }
}