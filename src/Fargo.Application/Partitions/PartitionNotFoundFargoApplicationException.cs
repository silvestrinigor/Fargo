namespace Fargo.Application.Exceptions;

/// <summary>
/// Exception thrown when a partition with the specified identifier cannot be found.
/// </summary>
public class PartitionNotFoundFargoApplicationException(Guid partitionGuid)
    : FargoApplicationException($"Partition with guid '{partitionGuid}' was not found.")
{
    /// <summary>
    /// Gets the identifier of the partition that could not be found.
    /// </summary>
    public Guid PartitionGuid { get; } = partitionGuid;
}
