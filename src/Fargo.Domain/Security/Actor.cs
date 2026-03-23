using Fargo.Domain.Entities;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Security;

/// <summary>
/// Provides a base implementation for <see cref="IActor"/>.
/// </summary>
/// <remarks>
/// The <see cref="Actor"/> class defines common behavior shared by all actor types,
/// such as permission and partition access checks.
///
/// Concrete implementations (e.g., <see cref="UserActor"/> and <see cref="SystemActor"/>)
/// are responsible for supplying the actor's identity and access data.
///
/// This class centralizes authorization logic to ensure consistency across the domain.
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
    /// <c>true</c> if the actor has permission for the specified action; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This default implementation checks whether the given action exists in
    /// <see cref="PermissionActions"/>.
    /// </remarks>
    public bool HasActionPermission(ActionType action)
    {
        return PermissionActions.Any(p => p == action);
    }

    /// <summary>
    /// Determines whether the actor has access to a specific partition.
    /// </summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <returns>
    /// <c>true</c> if the actor has access to the specified partition; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This default implementation checks whether the partition identifier exists in
    /// <see cref="PartitionAccesses"/>.
    /// </remarks>
    public bool HasPartitionAccess(Guid partitionGuid)
    {
        return PartitionAccesses.Any(p => p == partitionGuid);
    }

    /// <summary>
    /// Determines whether the actor has access to a partitioned resource.
    /// </summary>
    /// <param name="partitioned">The partitioned entity to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the actor has access to at least one partition of the entity;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method evaluates whether any partition associated with the given
    /// <see cref="IPartitioned"/> entity matches the actor's accessible partitions.
    /// </remarks>
    public bool HasAccess(IPartitioned partitioned)
    {
        return partitioned.Partitions.Any(p => PartitionAccesses.Contains(p.Guid));
    }
}
