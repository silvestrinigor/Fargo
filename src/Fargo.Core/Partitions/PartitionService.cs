namespace Fargo.Core.Partitions;

/// <summary>
/// Partition core service.
/// </summary>
public class PartitionService(IPartitionRepository partitionRepository)
{
    /// <summary>
    /// Validates that <paramref name="parentPartition"/> can be assigned as the
    /// parent of <paramref name="memberPartition"/>.
    /// </summary>
    /// <param name="parentPartition">The candidate parent partition.</param>
    /// <param name="memberPartition">The partition whose parent is being assigned.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown if assigning the parent would create a circular hierarchy.
    /// </exception>
    public async Task ValidateParentPartitionAssignmentAsync(
        Partition parentPartition, Partition memberPartition, CancellationToken cancellationToken = default)
    {
        var createsCircularHierarchy = await WouldCreateCircularHierarchyAsync(
            parentPartition, memberPartition.Guid, cancellationToken);

        if (createsCircularHierarchy)
        {
            throw new FargoCoreException(
                $"Partition '{memberPartition.Guid}' cannot be assigned to parent " +
                $"'{parentPartition.Guid}' because this would create a circular hierarchy.",
                FargoCoreErrorType.None);
        }
    }

    private async Task<bool> WouldCreateCircularHierarchyAsync(
        Partition candidateParentPartition, Guid memberPartitionGuid, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidateParentPartition);

        if (candidateParentPartition.Guid == memberPartitionGuid)
        {
            return true;
        }

        var descendantPartitionGuids =
            await partitionRepository.GetDescendantGuidsAsync(
                memberPartitionGuid, false, cancellationToken);

        return descendantPartitionGuids.Contains(candidateParentPartition.Guid);
    }

    /// <summary>
    /// Validates that the specified <paramref name="partition"/> can be deleted.
    /// </summary>
    /// <param name="partition">The partition to validate.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown if the partition is the global partition or if it has associated entities.
    /// </exception>
    public async Task ValidatePartitionCanBeDeletedAsync(Partition partition, CancellationToken cancellationToken = default)
    {
        if (partition.IsGlobalPartition)
        {
            throw new FargoCoreException(
                "The global partition cannot be deleted.",
                FargoCoreErrorType.None);
        }

        var hasAssociatedEntities = await partitionRepository.HasAnyAssociatedEntityAsync(partition.Guid, cancellationToken);

        if (hasAssociatedEntities)
        {
            throw new FargoCoreException(
                $"Partition '{partition.Guid}' cannot be deleted because it has associated entities.",
                FargoCoreErrorType.None);
        }
    }
}
