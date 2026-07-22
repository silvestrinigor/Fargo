namespace Fargo.Core.Partitions;

/// <summary>
/// Exception thrown when a partition hierarchy would become circular.
/// </summary>
public sealed class PartitionCircularHierarchyFargoDomainException(
    Guid parentPartitionGuid,
    Guid memberPartitionGuid
    ) : FargoCoreException(
        $"Partition '{memberPartitionGuid}' cannot be assigned to parent " +
        $"'{parentPartitionGuid}' because this would create a circular hierarchy.",
        FargoCoreErrorType.PartitionCircularHierarchy)
{
    /// <summary>
    /// Gets the identifier of the candidate parent partition.
    /// </summary>
    public Guid ParentPartitionGuid { get; } = parentPartitionGuid;

    /// <summary>
    /// Gets the identifier of the member partition.
    /// </summary>
    public Guid MemberPartitionGuid { get; } = memberPartitionGuid;
}
