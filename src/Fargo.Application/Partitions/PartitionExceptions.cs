namespace Fargo.Application.Partitions;

public class PartitionNotFoundFargoApplicationException(Guid partitionGuid)
    : FargoApplicationException($"Partition with guid '{partitionGuid}' was not found.")
{
    public Guid PartitionGuid { get; } = partitionGuid;
}

public sealed class PartitionAccessDeniedFargoApplicationException(
    Guid partitionGuid,
    Guid userGuid
) : FargoApplicationException(
    $"User '{userGuid}' does not have access to partition '{partitionGuid}'.")
{
    public Guid PartitionGuid { get; } = partitionGuid;

    public Guid UserGuid { get; } = userGuid;
}

public sealed class PartitionedEntityAccessDeniedFargoApplicationException(
    Guid entityGuid,
    Guid userGuid
) : FargoApplicationException(
    $"User '{userGuid}' does not have access to entity '{entityGuid}'.")
{
    public Guid EntityGuid { get; } = entityGuid;

    public Guid UserGuid { get; } = userGuid;
}
