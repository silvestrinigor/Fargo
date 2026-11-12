using Fargo.Core.Entities.Partitions;

namespace Fargo.Core.Entities.Actors;

/// <summary>
/// The resolved, runtime authorization context (permissions and partition access) of an
/// <see cref="Actor"/>; distinct from the persisted entity.
/// </summary>
public sealed class SecurityPrincipal
{
    /// <summary>
    /// The underlying actor's identifier.
    /// </summary>
    public Guid Guid { get; }

    public ActorType ActorType { get; }

    public IReadOnlySet<ActionType> Permissions => permissions.AsReadOnly();
    private readonly ISet<ActionType> permissions;

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

    public bool HasPermission(ActionType action)
    {
        return permissions.Contains(action);
    }

    public bool HasPartitionAccess(Guid partitionGuid)
    {
        return partitionAccessGuids.Contains(partitionGuid);
    }

    public bool HasAccess(Partition partition)
    {
        return HasPartitionAccess(partition.Guid);
    }

    /// <summary>
    /// A resource with no partitions is accessible; otherwise access to any one of its partitions
    /// is enough.
    /// </summary>
    public bool HasAccess(IPartitionedGuidsReadOnly partitioned)
    {
        if (partitioned.PartitionGuids.Count == 0)
        {
            return true;
        }

        return partitioned.PartitionGuids.Any(partitionAccessGuids.Contains);
    }
}
