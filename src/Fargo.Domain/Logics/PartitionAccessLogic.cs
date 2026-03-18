using Fargo.Domain.Entities;

namespace Fargo.Domain.Logics;

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
    public static bool HasAccess(this Partition partition, IPartitionUser partitionUser)
    {
        ArgumentNullException.ThrowIfNull(partition);
        ArgumentNullException.ThrowIfNull(partitionUser);

        return partitionUser.PartitionAccesses
            .Any(access => access.Partition == partition);
    }
}
