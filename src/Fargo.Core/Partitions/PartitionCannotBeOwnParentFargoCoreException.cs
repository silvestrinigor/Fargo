namespace Fargo.Core.Partitions;

/// <summary>
/// Exception thrown when a partition is assigned as its own parent.
/// </summary>
public sealed class PartitionCannotBeOwnParentFargoCoreException(
    Guid partitionGuid
    ) : FargoCoreException(
        $"Partition '{partitionGuid}' cannot be its own parent.",
        FargoCoreErrorType.PartitionCannotBeOwnParentPartition)
{
    /// <summary>
    /// Gets the identifier of the partition involved in the violation.
    /// </summary>
    public Guid PartitionGuid { get; } = partitionGuid;
}
