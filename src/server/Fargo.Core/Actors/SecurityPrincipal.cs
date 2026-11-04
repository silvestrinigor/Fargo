using Fargo.Core.Partitions;

namespace Fargo.Core.Actors;

/// <summary>
/// Represents the authenticated security principal performing operations within
/// the system.
/// </summary>
/// <remarks>
/// A security principal encapsulates the identity, permissions, and partition
/// access of the current execution context. It is used by the domain and
/// application layers to determine whether an operation is authorized.
///
/// This is the resolved, runtime authorization context for an <see cref="Actor"/>
/// (a user or a service account); it is distinct from the persisted
/// <see cref="Actor"/> entity.
/// </remarks>
public sealed class SecurityPrincipal
{
    /// <summary>
    /// Gets the unique identifier of the principal (the underlying actor guid).
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Gets the type of actor.
    /// </summary>
    public ActorType ActorType { get; }

    /// <summary>
    /// Gets the permissions granted to the principal.
    /// </summary>
    public IReadOnlySet<ActionType> Permissions => permissions.AsReadOnly();
    private readonly ISet<ActionType> permissions;

    /// <summary>
    /// Gets the identifiers of the partitions the principal is authorized to access.
    /// </summary>
    public IReadOnlySet<Guid> PartitionAccessGuids => partitionAccessGuids.AsReadOnly();

    private readonly ISet<Guid> partitionAccessGuids;

    internal SecurityPrincipal(
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
    /// Determines whether the principal has the specified permission.
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
    /// Determines whether the principal has access to the specified partition.
    /// </summary>
    /// <param name="partitionGuid">The identifier of the partition.</param>
    /// <returns>
    /// <see langword="true"/> if the principal has access to the partition;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool HasPartitionAccess(Guid partitionGuid)
    {
        return partitionAccessGuids.Contains(partitionGuid);
    }

    /// <summary>
    /// Determines whether the principal has access to the specified partition.
    /// </summary>
    /// <param name="partition">The partition to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if the principal has access to the partition;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool HasAccess(Partition partition)
    {
        return HasPartitionAccess(partition.Guid);
    }

    /// <summary>
    /// Determines whether access is granted to the specified partitioned resource.
    /// </summary>
    /// <param name="partitioned">
    /// The resource containing the partition identifiers that are required to access it.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the resource is not associated with any partitions,
    /// or if at least one of its partitions is included in the partitions accessible
    /// to the current principal; otherwise, <see langword="false"/>.
    /// </returns>
    public bool HasAccess(IPartitionedGuidsReadOnly partitioned)
    {
        if (partitioned.PartitionGuids.Count == 0)
        {
            return true;
        }

        return partitioned.PartitionGuids.Any(partitionAccessGuids.Contains);
    }
}
