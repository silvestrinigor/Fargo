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

        var partition = await partitionRepository.GetByGuid(
                partitionGuid,
                cancellationToken
                );

        if (partition != null &&
            !await HasAccess(
                partition,
                actor,
                cancellationToken
                ))
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
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the user has access to the partition,
    /// either directly or through one of its ancestor partitions, whether
    /// granted directly or through one of the user's group memberships;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> or <paramref name="user"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// Access inheritance flows from parent to child. Therefore, a user with
    /// access to an ancestor partition also has access to the evaluated partition.
    /// </remarks>
    public async Task<bool> HasAccess(
            Partition partition,
            User user,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(partition);
        ArgumentNullException.ThrowIfNull(user);

        var visitedPartitionGuids = new HashSet<Guid>();
        Partition? currentPartition = partition;

        while (currentPartition != null)
        {
            if (!visitedPartitionGuids.Add(currentPartition.Guid))
            {
                throw new InvalidOperationException(
                        $"Partition hierarchy contains a cycle involving " +
                        $"partition '{currentPartition.Guid}'."
                        );
            }

            var userHasAccess = currentPartition.HasAccess(user);

            if (userHasAccess)
            {
                return true;
            }

            var userGroupHasAccess = user.UserGroups.Any(currentPartition.HasAccess);

            if (userGroupHasAccess)
            {
                return true;
            }

            var parentPartitionGuid = currentPartition.ParentPartitionGuid;

            if (parentPartitionGuid is null)
            {
                return false;
            }

            if (currentPartition.ParentPartition?.Guid == parentPartitionGuid.Value)
            {
                currentPartition = currentPartition.ParentPartition;
                continue;
            }

            currentPartition =
                await partitionRepository.GetByGuid(
                        parentPartitionGuid.Value,
                        cancellationToken
                        )
                ?? throw new InvalidOperationException(
                        $"Partition hierarchy is inconsistent. " +
                        $"Partition '{parentPartitionGuid}' was not found."
                        );
        }

        return false;
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
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the user has access to the entity,
    /// either directly or through one of their group memberships,
    /// including access inherited from ancestor partitions; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partitioned"/> or <paramref name="user"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method evaluates access against the partitions associated with the
    /// entity. Effective access may come from the user's own partition access
    /// entries or from any group the user belongs to. If the entity has no
    /// associated partitions, access is granted.
    /// </remarks>
    public async Task<bool> HasAccess(
            IPartitioned partitioned,
            User user,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(partitioned);
        ArgumentNullException.ThrowIfNull(user);

        foreach (var partition in partitioned.Partitions)
        {
            if (await HasAccess(partition, user, cancellationToken))
            {
                return true;
            }
        }

        return partitioned.Partitions.Count > 0;
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
