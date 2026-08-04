namespace Fargo.Core.Partitions;

/// <summary>
/// Provides domain operations and validation rules for <see cref="Partition"/> entities.
/// </summary>
/// <remarks>
/// This service contains business rules that require repository access and
/// therefore cannot be enforced by the <see cref="Partition"/> aggregate alone.
/// </remarks>
public class PartitionService(IPartitionRepository partitionRepository)
{
    /// <summary>
    /// Ensures that assigning the specified parent partition to the specified
    /// member partition would not create a circular hierarchy.
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
        var createsCircularHierarchy = await CreatesCircularHierarchyAsync(
            parentPartition, memberPartition.Guid, cancellationToken);

        if (createsCircularHierarchy)
        {
            CircularHierarchy(memberPartition.Guid, parentPartition.Guid);
        }
    }

    private static FargoCoreException CircularHierarchy(Guid parent, Guid child) =>
        new(
            $"Partition '{child}' cannot be assigned to parent '{parent}' because this would create a circular hierarchy.",
            FargoCoreErrorType.None);

    /// <summary>
    /// Determines whether assigning the specified partition as a parent
    /// would create a circular hierarchy.
    /// </summary>
    /// <param name="candidateParentPartition">
    /// The partition that would become the parent.
    /// </param>
    /// <param name="memberPartitionGuid">
    /// The identifier of the partition receiving the new parent.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the assignment would create a circular
    /// hierarchy; otherwise, <see langword="false"/>.
    /// </returns>
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
    /// Ensures that the specified partition can be safely deleted.
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
