using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

/// <summary>
/// Represents the authenticated actor performing operations within the system.
/// </summary>
/// <remarks>
/// An actor encapsulates the identity, permissions, and partition access of the
/// current execution context. It is used by the domain and application layers
/// to determine whether an operation is authorized.
/// </remarks>
public sealed class Actor
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Gets the type of actor.
    /// </summary>
    public ActorType ActorType { get; }

    /// <summary>
    /// Gets the permissions granted to the actor.
    /// </summary>
    public IReadOnlySet<ActionType> Permissions => permissions.AsReadOnly();
    private readonly ISet<ActionType> permissions;

    /// <summary>
    /// Gets the identifiers of the partitions the actor is authorized to access.
    /// </summary>
    public IReadOnlySet<Guid> PartitionAccessGuids => partitionAccessGuids.AsReadOnly();

    private readonly ISet<Guid> partitionAccessGuids;

    internal Actor(
        Guid actorGuid,
        ActorType actorType,
        ISet<ActionType> permissions,
        ISet<Guid> partitionAccess)
    {
        Guid = actorGuid;
        ActorType = actorType;
        this.permissions = permissions;
        partitionAccessGuids = partitionAccess;
    }

    /// <summary>
    /// Determines whether the actor has the specified permission.
    /// </summary>
    /// <param name="action">The permission to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if the permission is granted; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool HasPermission(ActionType action)
    {
        return permissions.Contains(action);
    }

    /// <summary>
    /// Determines whether the actor has access to the specified partition.
    /// </summary>
    /// <param name="partitionGuid">The identifier of the partition.</param>
    /// <returns>
    /// <see langword="true"/> if the actor has access to the partition;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool HasPartitionAccess(Guid partitionGuid)
    {
        return partitionAccessGuids.Contains(partitionGuid);
    }

    /// <summary>
    /// Determines whether the actor has access to the specified partition.
    /// </summary>
    /// <param name="partition">The partition to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if the actor has access to the partition;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool HasAccess(Partition partition)
    {
        return HasPartitionAccess(partition.Guid);
    }

    public bool HasAccess(IPartitionedGuidsReadOnly partitioned)
    {
        if (partitioned.PartitionGuids.Count == 0)
        {
            return true;
        }

        return partitioned.PartitionGuids.Any(partitionAccessGuids.Contains);
    }
}
