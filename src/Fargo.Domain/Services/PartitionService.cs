using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Logics;
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
/// </remarks>
public class PartitionService(
        IPartitionRepository partitionRepository)
{
    /// <summary>
    /// Gets a partition by its identifier if the specified actor has access to it.
    /// </summary>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition to retrieve.
    /// </param>
    /// <param name="actor">
    /// The user performing the operation. The actor must have effective access
    /// to the requested partition, either directly or through a group membership.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="Partition"/> when it exists and the actor has
    /// access to it; otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="actor"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method enforces partition-based visibility rules.
    /// A partition is returned only when the requesting user has effective
    /// access to it according to the domain access rules.
    /// </remarks>
    public async Task<Partition?> GetPartition(
            Guid partitionGuid,
            User actor,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var partition = await partitionRepository.GetByGuid(partitionGuid, cancellationToken);

        if (partition != null && !HasAccess(partition, actor))
        {
            return null;
        }

        return partition;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="user"/> has effective
    /// access to the given <paramref name="partition"/>.
    /// </summary>
    /// <param name="partition">
    /// The partition to evaluate access for.
    /// </param>
    /// <param name="user">
    /// The user whose access is being evaluated.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the user has access to the partition,
    /// either directly or through one of their group memberships;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> or <paramref name="user"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static bool HasAccess(Partition partition, User user)
    {
        ArgumentNullException.ThrowIfNull(partition);
        ArgumentNullException.ThrowIfNull(user);

        var userHasAccess = partition.HasAccess(user);

        if (userHasAccess)
        {
            return true;
        }

        var userGroupHasAccess = user.UserGroups.Any(partition.HasAccess);

        return userGroupHasAccess;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="user"/> has effective
    /// access to the given partitioned entity.
    /// </summary>
    /// <param name="partitioned">
    /// The partitioned entity whose access is being evaluated.
    /// </param>
    /// <param name="user">
    /// The user whose access is being evaluated.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the user has access to the entity,
    /// either directly or through one of their group memberships;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partitioned"/> or <paramref name="user"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method evaluates access against the partitions associated with the
    /// entity. Effective access may come from the user's own partition access
    /// entries or from any group the user belongs to.
    /// </remarks>
    public static bool HasAccess(IPartitioned partitioned, User user)
    {
        ArgumentNullException.ThrowIfNull(partitioned);
        ArgumentNullException.ThrowIfNull(user);

        var userHasAccess = partitioned.HasAccess(user);

        if (userHasAccess)
        {
            return true;
        }

        var userGroupHasAccess = user.UserGroups.Any(partitioned.HasAccess);

        return userGroupHasAccess;
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
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(parentPartition);
        ArgumentNullException.ThrowIfNull(memberPartition);

        if (parentPartition.Guid == memberPartition.Guid)
        {
            throw new PartitionCannotBeOwnParentFargoDomainException(
                    memberPartition.Guid
                    );
        }

        var createsCircularHiearchy =
            await CreatesCircularHiearchy(
                    parentPartition,
                    memberPartition.Guid,
                    cancellationToken
                    );

        if (createsCircularHiearchy)
        {
            throw new PartitionCircularHierarchyFargoDomainException(
                    parentPartition.Guid,
                    memberPartition.Guid
                    );
        }

        memberPartition.ParentPartition = parentPartition;
    }

    private async Task<bool> CreatesCircularHiearchy(
            Partition candidateParentPartition,
            Guid memberPartitionGuid,
            CancellationToken cancellationToken
            )
    {
        var visitedPartitionGuids = new HashSet<Guid>();
        Partition? currentPartition = candidateParentPartition;

        while (currentPartition != null)
        {
            if (!visitedPartitionGuids.Add(currentPartition.Guid))
            {
                return true;
            }

            if (currentPartition.Guid == memberPartitionGuid)
            {
                return true;
            }

            var nextParentGuid = currentPartition.ParentPartitionGuid;
            if (nextParentGuid is null)
            {
                return false;
            }

            if (currentPartition.ParentPartition?.Guid == nextParentGuid.Value)
            {
                currentPartition = currentPartition.ParentPartition;
                continue;
            }

            currentPartition =
                await partitionRepository.GetByGuid(
                        nextParentGuid.Value,
                        cancellationToken
                        )
                ?? throw new InvalidOperationException(
                        $"Partition hierarchy is inconsistent. " +
                        $"Partition '{nextParentGuid}' was not found."
                        );
        }

        return false;
    }
}
