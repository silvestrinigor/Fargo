using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

/// <summary>
/// Represents an actor responsible for performing operations within the system.
/// </summary>
/// <remarks>
/// An actor abstracts the authenticated user responsible for an action.
/// Authorization is evaluated against the actor's permissions and partition access.
/// </remarks>
public sealed class Actor
{
    public Guid Guid { get; }

    public ActorType ActorType { get; }

    public IReadOnlySet<ActionType> Permissions => permissions.AsReadOnly();
    private readonly ISet<ActionType> permissions;

    public IReadOnlySet<Guid> PartitionAccessGuids => partitionAccessGuids.AsReadOnly();
    private readonly ISet<Guid> partitionAccessGuids;

    internal Actor(Guid actorGuid, ActorType actorType, ISet<ActionType> permissions, ISet<Guid> partitionAccess)
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

    public bool HasAccess(IPartitionedReadOnly partitioned)
    {
        if (partitioned.Partitions.Count == 0)
        {
            return true;
        }

        return partitioned.Partitions.Any(p => partitionAccessGuids.Contains(p.Guid));
    }
}
