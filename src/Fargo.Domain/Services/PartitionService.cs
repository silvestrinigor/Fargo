using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services;

/// <summary>
/// Provides domain operations related to partition retrieval
/// and partition-based access evaluation.
/// </summary>
/// <remarks>
/// This service encapsulates logic for retrieving partitions while enforcing
/// access rules based on the effective partition access of a <see cref="User"/>.
///
/// Effective access may be granted either:
/// <list type="bullet">
/// <item>
/// <description>directly to the user</description>
/// </item>
/// <item>
/// <description>indirectly through one of the user's <see cref="UserGroup"/> memberships</description>
/// </item>
/// </list>
///
/// Access inheritance flows from parent to child. This means that a user
/// with access to a parent partition also has access to all of its descendant
/// partitions. Access does not flow from child to parent.
/// </remarks>
public class PartitionService(
        IPartitionRepository partitionRepository)
{
    /// <summary>
    /// The predefined unique identifier string representing
    /// the global partition.
    /// </summary>
    /// <remarks>
    /// The global partition is the root of the partition hierarchy
    /// and has implicit access to all descendant partitions.
    /// </remarks>
    private const string GlobalPartitionGuidString =
        "00000000-0000-0000-0000-000000000002";

    /// <summary>
    /// Gets the predefined unique identifier representing
    /// the global partition.
    /// </summary>
    /// <remarks>
    /// This GUID is reserved for the root partition of the system.
    /// It must remain constant across environments and is used
    /// to establish the top-level access scope for privileged users.
    /// </remarks>
    public static Guid GlobalPartitionGuid =>
        new(GlobalPartitionGuidString);

    /// <summary>
    /// Deletes the specified <see cref="Partition"/> from the system.
    /// </summary>
    /// <param name="partition">
    /// The partition to be deleted.
    /// </param>
    /// <exception cref="PartitionGlobalDeleteFargoDomainException">
    /// Thrown when an attempt is made to delete the global partition.
    /// </exception>
    /// <remarks>
    /// This operation removes the partition from the system.
    /// </remarks>
    /// <remarks>
    /// <b>Important:</b>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The global partition cannot be deleted under any circumstances.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public void DeletePartition(Partition partition)
    {
        if (partition.Guid == GlobalPartitionGuid)
        {
            throw new PartitionGlobalDeleteFargoDomainException();
        }

        partitionRepository.Remove(partition);
    }

    /// <summary>
    /// Sets the parent partition of a member partition.
    /// </summary>
    /// <param name="parentPartition">
    /// The partition that will become the parent.
    /// </param>
    /// <param name="memberPartition">
    /// The partition that will become a child of <paramref name="parentPartition"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parentPartition"/> or
    /// <paramref name="memberPartition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="PartitionCannotBeOwnParentFargoDomainException">
    /// Thrown when a partition is assigned as its own parent.
    /// </exception>
    /// <exception cref="PartitionCircularHierarchyFargoDomainException">
    /// Thrown when assigning the parent would create a circular hierarchy.
    /// </exception>
    public async Task SetParentPartition(
            Partition parentPartition,
            Partition memberPartition,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentPartition);
        ArgumentNullException.ThrowIfNull(memberPartition);

        if (parentPartition.Guid == memberPartition.Guid)
        {
            throw new PartitionCannotBeOwnParentFargoDomainException(
                memberPartition.Guid
            );
        }

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

        memberPartition.ParentPartition = parentPartition;
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
