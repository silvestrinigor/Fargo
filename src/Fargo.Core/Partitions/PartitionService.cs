namespace Fargo.Core.Partitions;

/// <summary>
/// Provides domain operations and validations that require access to
/// <see cref="Partition"/> persistence or multiple partitions.
/// </summary>
/// <remarks>
/// This service contains business rules that require repository access and
/// therefore cannot be enforced by the <see cref="Partition"/> aggregate alone.
/// </remarks>
public sealed class PartitionService(IPartitionRepository partitionRepository)
{
    /// <summary>
    /// Validates that assigning the specified parent partition to the specified
    /// child partition would result in a valid partition hierarchy.
    /// </summary>
    /// <param name="parentPartition">
    /// The partition that will become the parent.
    /// </param>
    /// <param name="childPartition">
    /// The partition that will become the child.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parentPartition"/> or
    /// <paramref name="childPartition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when the assignment would create a circular partition hierarchy.
    /// </exception>
    /// <remarks>
    /// This method should be called before
    /// <see cref="Partition.SetParentPartition(Partition)"/> because validating
    /// the complete hierarchy requires access to other partitions through the
    /// repository.
    /// </remarks>
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
                FargoCoreErrorType.InvalidOperation);
        }
    }

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
        ArgumentNullException.ThrowIfNull(partition);

        if (partition.IsGlobalPartition)
        {
            throw new FargoCoreException(
                $"The global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}' cannot be deleted.",
                FargoCoreErrorType.InvalidOperation);
        }

        var hasAssociatedEntities = await partitionRepository.HasAnyAssociatedEntityAsync(partition.Guid, cancellationToken);

        if (hasAssociatedEntities)
        {
            throw new FargoCoreException(
                $"Partition '{partition.Guid}' cannot be deleted because it has associated entities.",
                FargoCoreErrorType.InvalidOperation);
        }
    }
}
