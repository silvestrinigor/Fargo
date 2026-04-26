using Fargo.Domain.Partitions;

namespace Fargo.Domain;

/// <summary>
/// Provides a base implementation for <see cref="IActor"/>.
/// </summary>
/// <remarks>
/// The <see cref="Actor"/> class defines common authorization behavior shared by all actor types,
/// including permission and partition access evaluation.
///
/// Concrete implementations (e.g., <see cref="Fargo.Domain.Users.UserActor"/> and <see cref="Fargo.Domain.System.SystemActor"/>)
/// are responsible for supplying identity and access data.
///
/// Authorization rules follow a hierarchical model:
/// <list type="number">
/// <item><description><b>System actors</b> have unrestricted access to all operations and partitions</description></item>
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
    /// Gets a value indicating whether the actor represents the system.
    /// </summary>
    public abstract bool IsSystem { get; }

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
    public abstract IReadOnlyCollection<Guid> PartitionAccesses { get; }

    /// <summary>
    /// Determines whether the actor has permission to perform a specific action.
    /// </summary>
    /// <param name="action">The action to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the actor is a system or administrative actor, or if the action
    /// is explicitly granted; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Evaluation order:
    /// <list type="number">
    /// <item><description>System actors are always authorized</description></item>
    /// <item><description>Administrative actors are always authorized</description></item>
    /// <item><description>Otherwise, checks <see cref="PermissionActions"/></description></item>
    /// </list>
    /// </remarks>
    public bool HasActionPermission(ActionType action)
    {
        if (IsSystem || IsAdmin)
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
    /// <c>true</c> if the actor is a system or administrative actor, or if the partition
    /// is explicitly accessible; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Evaluation order:
    /// <list type="number">
    /// <item><description>System actors have unrestricted access</description></item>
    /// <item><description>Administrative actors have unrestricted access</description></item>
    /// <item><description>Otherwise, checks <see cref="PartitionAccesses"/></description></item>
    /// </list>
    /// </remarks>
    public bool HasPartitionAccess(Guid partitionGuid)
    {
        if (IsSystem || IsAdmin)
        {
            return true;
        }

        return PartitionAccesses.Contains(partitionGuid);
    }

    /// <summary>
    /// Determines whether the actor has access to a partitioned resource.
    /// </summary>
    /// <param name="partitioned">The partitioned entity to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the actor is a system or administrative actor, if the entity has no
    /// partitions (public), or if at least one partition of the entity is accessible;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Evaluation order:
    /// <list type="number">
    /// <item><description>System actors have unrestricted access</description></item>
    /// <item><description>Administrative actors have unrestricted access</description></item>
    /// <item><description>Entities with no partitions are public and accessible to all authenticated actors</description></item>
    /// <item><description>Otherwise, checks intersection with <see cref="PartitionAccesses"/></description></item>
    /// </list>
    /// </remarks>
    public bool HasAccess(IPartitionedEntity partitioned)
    {
        if (IsSystem || IsAdmin)
        {
            return true;
        }

        if (partitioned.Partitions.Count == 0)
        {
            return true;
        }

        return partitioned.Partitions.Any(p => PartitionAccesses.Contains(p.Guid));
    }
}
