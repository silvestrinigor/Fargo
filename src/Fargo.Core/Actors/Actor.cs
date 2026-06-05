using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Users;

namespace Fargo.Core.Actors;

/// <summary>
/// Represents an actor responsible for performing operations within the system.
/// </summary>
/// <remarks>
/// An actor abstracts the authenticated user responsible for an action.
/// Authorization is evaluated against the actor's permissions and partition
/// access, while auditing is handled separately by the infrastructure layer.
/// </remarks>
public sealed class Actor
{
    public Actor(
        Guid guid,
        bool isAdmin,
        bool isActive,
        IReadOnlyCollection<ActionType> permissionActions,
        IReadOnlyCollection<Guid> partitionAccessesGuids)
    {
        ArgumentNullException.ThrowIfNull(permissionActions);
        ArgumentNullException.ThrowIfNull(partitionAccessesGuids);

        Guid = guid;
        IsAdmin = isAdmin;
        IsActive = isActive;
        PermissionActions = permissionActions;
        PartitionAccessesGuids = partitionAccessesGuids;
    }

    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Gets a value indicating whether the actor has administrative privileges.
    /// </summary>
    public bool IsAdmin { get; }

    /// <summary>
    /// Gets a value indicating whether the actor is active.
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Gets the set of actions the actor is permitted to perform.
    /// </summary>
    public IReadOnlyCollection<ActionType> PermissionActions { get; }

    /// <summary>
    /// Gets the set of partitions the actor has access to.
    /// </summary>
    public IReadOnlyCollection<Guid> PartitionAccessesGuids { get; }

    /// <summary>
    /// Determines whether the actor has permission to perform a specific action.
    /// </summary>
    /// <param name="action">The action to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the actor is an administrative actor, or if the action
    /// is explicitly granted; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Administrative actors are always authorized; otherwise the actor's
    /// effective permissions are checked.
    /// </remarks>
    public bool HasActionPermission(ActionType action)
    {
        if (IsAdmin)
        {
            return true;
        }

        return PermissionActions.Contains(action);
    }

    /// <summary>
    /// Determines whether the actor has access to a specific partition.
    /// </summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <returns>
    /// <c>true</c> if the actor is an administrative actor, or if the partition
    /// is explicitly accessible; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Administrative actors have unrestricted access; otherwise the actor's
    /// partition list is checked.
    /// </remarks>
    public bool HasPartitionAccess(Guid partitionGuid)
    {
        if (IsAdmin)
        {
            return true;
        }

        return PartitionAccessesGuids.Contains(partitionGuid);
    }

    /// <summary>
    /// Determines whether the actor has access to a partitioned resource.
    /// </summary>
    /// <param name="partitioned">The partitioned entity to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the actor is an administrative actor, if the entity has no
    /// partitions (public), or if at least one partition of the entity is accessible;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Entities with no partitions are public and accessible to all authenticated
    /// actors. Otherwise, the actor's partition access is checked.
    /// </remarks>
    public bool HasAccess(IPartitionedEntity partitioned)
    {
        if (IsAdmin)
        {
            return true;
        }

        if (partitioned.Partitions.Count == 0)
        {
            return true;
        }

        return partitioned.Partitions.Any(p => PartitionAccessesGuids.Contains(p.Guid));
    }

    public bool HasAccess(IPartitioned partitioned)
    {
        if (IsAdmin)
        {
            return true;
        }

        if (partitioned.PartitionGuids.Count == 0)
        {
            return true;
        }

        return partitioned.PartitionGuids.Any(p => PartitionAccessesGuids.Contains(p));
    }

    public void ValidateHasPermission(ActionType action)
    {
        if (!HasActionPermission(action))
        {
            throw new UserNotAuthorizedFargoDomainException(Guid, action);
        }
    }

    public void ValidateHasPartitionAccess(Guid partitionGuid)
    {
        if (!HasPartitionAccess(partitionGuid))
        {
            throw new UserPartitionAccessNotAuthorizedFargoDomainException(Guid, partitionGuid);
        }
    }

    public void ValidateHasAccess<TEntity>(TEntity partitioned)
        where TEntity : IEntity, IPartitionedEntity
    {
        ArgumentNullException.ThrowIfNull(partitioned);

        if (!HasAccess(partitioned))
        {
            throw new UserEntityAccessNotAuthorizedFargoDomainException(Guid, partitioned.Guid);
        }
    }
}
