namespace Fargo.Application.Partitions;

/// <summary>
/// Exception thrown when a user attempts to access or use a partition
/// they do not have permission to access.
/// </summary>
public sealed class PartitionAccessDeniedFargoApplicationException(
        Guid partitionGuid,
        Guid userGuid
        )
    : FargoApplicationException(
        $"User '{userGuid}' does not have access to partition '{partitionGuid}'."
        )
{
    /// <summary>
    /// Gets the unique identifier of the partition that was denied.
    /// </summary>
    public Guid PartitionGuid { get; } = partitionGuid;

    /// <summary>
    /// Gets the unique identifier of the user who attempted the operation.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
