namespace Fargo.Domain.Exceptions;

/// <summary>
/// Exception thrown when a partition is assigned as its own parent.
/// </summary>
public sealed class PartitionCannotBeOwnParentFargoDomainException(
        Guid partitionGuid
        ) : FargoDomainException(
            $"Partition '{partitionGuid}' cannot be its own parent."
            )
{
    /// <summary>
    /// Gets the identifier of the partition involved in the violation.
    /// </summary>
    public Guid PartitionGuid { get; } = partitionGuid;
}
