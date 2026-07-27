namespace Fargo.Core.Partitions;

public class PartitionService(
    IPartitionRepository partitionRepository)
{
    public async Task ValidateHierarchyParentPartition(
        Partition parentPartition,
        Partition memberPartition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentPartition);
        ArgumentNullException.ThrowIfNull(memberPartition);

        var createsCircularHierarchy =
            await CreatesCircularHierarchy(
                parentPartition,
                memberPartition.Guid,
                cancellationToken
            );

        if (createsCircularHierarchy)
        {
            throw new PartitionCircularHierarchyFargoDomainException(
                parentPartition.Guid,
                memberPartition.Guid
            );
        }
    }

    private async Task<bool> CreatesCircularHierarchy(
        Partition candidateParentPartition,
        Guid memberPartitionGuid,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidateParentPartition);

        if (candidateParentPartition.Guid == memberPartitionGuid)
        {
            return true;
        }

        var descendantPartitionGuids =
            await partitionRepository.GetDescendantGuids(
                memberPartitionGuid,
                false,
                cancellationToken
            );

        return descendantPartitionGuids.Contains(candidateParentPartition.Guid);
    }
}
