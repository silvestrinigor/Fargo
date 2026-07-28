namespace Fargo.Core.Partitions;

public sealed class PartitionDeleteWithEntitiesAssociatedFargoCoreException(Guid partitionGuid)
    : FargoCoreException(
        $"Partition '{partitionGuid}' cannot be deleted because it has associated entities.",
        FargoCoreErrorType.CannotDeletePartitionWithEntitiesAssociated)
{
    public Guid PartitionGuid { get; } = partitionGuid;
}
