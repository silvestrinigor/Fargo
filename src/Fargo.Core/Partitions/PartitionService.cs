namespace Fargo.Core.Partitions;

public class PartitionService(IPartitionRepository partitionRepository)
{
    public async Task ValidateHierarchyParentPartition(
        Partition parentPartition, Partition memberPartition, CancellationToken cancellationToken = default)
    {
        var createsCircularHierarchy = await CreatesCircularHierarchy(
            parentPartition, memberPartition.Guid, cancellationToken);

        if (createsCircularHierarchy)
        {
            throw new FargoCoreException(
                $"Partition '{memberPartition.Guid}' cannot be assigned to parent " +
                $"'{parentPartition.Guid}' because this would create a circular hierarchy.",
                FargoCoreErrorType.None);
        }
    }

    private async Task<bool> CreatesCircularHierarchy(
        Partition candidateParentPartition, Guid memberPartitionGuid, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidateParentPartition);

        if (candidateParentPartition.Guid == memberPartitionGuid)
        {
            return true;
        }

        var descendantPartitionGuids =
            await partitionRepository.GetDescendantGuids(
                memberPartitionGuid, false, cancellationToken);

        return descendantPartitionGuids.Contains(candidateParentPartition.Guid);
    }

    public async Task ValidatePartitionDelete(Partition partition, CancellationToken cancellationToken = default)
    {
        if (partition.IsGlobalPartition)
        {
            throw new FargoCoreException(
                "The global partition cannot be deleted.",
                FargoCoreErrorType.None);
        }

        var hasAssociatedEntities = await partitionRepository.HasAnyAssociatedEntity(partition.Guid, cancellationToken);

        if (hasAssociatedEntities)
        {
            throw new FargoCoreException(
                $"Partition '{partition.Guid}' cannot be deleted because it has associated entities.",
                FargoCoreErrorType.None);
        }
    }
}
