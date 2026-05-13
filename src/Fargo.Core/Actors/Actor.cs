using Fargo.Core.Partitions;

namespace Fargo.Core.Actors;

/// <summary>
/// Represents an actor responsible for performing operations within the system.
/// </summary>
/// <remarks>
/// An actor abstracts the authenticated user responsible for an action.
/// Authorization is evaluated against the actor's permissions and partition
/// access, while auditing is handled separately by the infrastructure layer.
/// </remarks>
public interface IActor
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> that uniquely identifies the actor instance.
    /// </value>
    Guid Guid { get; }

    /// <summary>
    /// Gets a value indicating whether the actor has administrative privileges.
    /// </summary>
    /// <value>
    /// <c>true</c> if the actor is an administrator; otherwise, <c>false</c>.
    /// </value>
    bool IsAdmin { get; }

    /// <summary>
    /// Gets a value indicating whether the actor is active.
    /// </summary>
    /// <value>
    /// <c>true</c> if the actor is active and allowed to perform actions; otherwise, <c>false</c>.
    /// </value>
    bool IsActive { get; }

    /// <summary>
    /// Gets the set of actions the actor is permitted to perform.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="ActionType"/> representing allowed operations.
    /// </value>
    IReadOnlyCollection<ActionType> PermissionActions { get; }

    /// <summary>
    /// Gets the set of partitions the actor has access to.
    /// </summary>
    /// <value>
    /// A read-only collection of partition identifiers (<see cref="Guid"/>).
    /// </value>
    IReadOnlyCollection<Guid> PartitionAccessesGuids { get; }

    /// <summary>
    /// Determines whether the actor has access to a specific partition.
    /// </summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <returns>
    /// <c>true</c> if the actor has access to the specified partition; otherwise, <c>false</c>.
    /// </returns>
    bool HasPartitionAccess(Guid partitionGuid);

    /// <summary>
    /// Determines whether the actor has permission to perform a specific action.
    /// </summary>
    /// <param name="action">The action to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the actor has permission for the specified action; otherwise, <c>false</c>.
    /// </returns>
    bool HasActionPermission(ActionType action);
}

/// <summary>
/// Provides a base implementation for <see cref="IActor"/>.
/// </summary>
/// <remarks>
/// The <see cref="Actor"/> class defines common authorization behavior shared by
/// user-backed actor types, including permission and partition access evaluation.
///
/// Authorization rules follow a hierarchical model:
/// <list type="number">
/// <item><description><b>Administrative actors</b> have unrestricted access within the domain</description></item>
/// <item><description>All other actors are evaluated based on their assigned permissions and partition access</description></item>
/// </list>
/// </remarks>
public abstract class Actor : IActor
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    public abstract Guid Guid { get; }

    /// <summary>
    /// Gets a value indicating whether the actor has administrative privileges.
    /// </summary>
    public abstract bool IsAdmin { get; }

    /// <summary>
    /// Gets a value indicating whether the actor is active.
    /// </summary>
    public abstract bool IsActive { get; }

    /// <summary>
    /// Gets the set of actions the actor is permitted to perform.
    /// </summary>
    public abstract IReadOnlyCollection<ActionType> PermissionActions { get; }

    /// <summary>
    /// Gets the set of partitions the actor has access to.
    /// </summary>
    public abstract IReadOnlyCollection<Guid> PartitionAccessesGuids { get; }

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
}
