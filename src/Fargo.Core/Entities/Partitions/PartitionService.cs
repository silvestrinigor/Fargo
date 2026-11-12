using Fargo.Core.Common;

namespace Fargo.Core.Entities.Partitions;

/// <summary>
/// Partition rules that need to read other partitions, so <see cref="Partition"/> cannot
/// enforce them alone.
/// </summary>
public sealed class PartitionService(IPartitionRepository partitionRepository)
{
    /// <summary>
    /// Call before <see cref="Partition.SetParentPartition(Partition)"/>, which cannot detect
    /// cycles on its own. Hold <see cref="IPartitionRepository.AcquireHierarchyLockAsync"/> so
    /// concurrent re-parents cannot each pass and together create a cycle.
    /// </summary>
    /// <exception cref="FargoCoreException">The assignment would create a cycle.</exception>
    public async Task ValidateParentPartitionHierarchyAssignmentAsync(
        Partition parentPartition, Partition childPartition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentPartition);
        ArgumentNullException.ThrowIfNull(childPartition);

        var createsCircularHierarchy = await CreatesCircularHierarchyAsync(
            parentPartition, childPartition.Guid, cancellationToken);

        if (createsCircularHierarchy)
        {
            throw new FargoCoreException(
                $"Partition '{childPartition.Guid}' cannot be assigned to parent partition '{parentPartition.Guid}' because this would create a circular hierarchy.",
                FargoErrorType.InvalidOperation);
        }
    }

    private async Task<bool> CreatesCircularHierarchyAsync(
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
    /// A partition cannot be deleted while it has child partitions, which would be left without
    /// their required parent.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The partition is the global partition or has direct child partitions.
    /// </exception>
    public async Task ValidatePartitionCanBeDeletedAsync(Partition partition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partition);

        partition.ValidateCanBeDeleted();

        var hasChildrenPartitions = await partitionRepository.HasChildrenAsync(partition.Guid, cancellationToken);

        if (hasChildrenPartitions)
        {
            throw new FargoCoreException(
                $"Partition '{partition.Guid}' cannot be deleted because it has children partitions.",
                FargoErrorType.InvalidOperation);
        }
    }
}
