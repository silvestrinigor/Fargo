using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

/// <summary>
/// Represents an actor responsible for performing operations within the system.
/// </summary>
/// <remarks>
/// An actor abstracts the authenticated user responsible for an action.
/// Authorization is evaluated against the actor's permissions and partition
/// access, while auditing is handled separately by the infrastructure layer.
/// </remarks>
public sealed class Actor : IActor
{
    public ActorId ActorId { get; }

    private readonly ISet<ActionType> permissionActionTypes;

    private readonly ISet<Guid> partitionAccessGuids;

    internal Actor(ActorId actorId, ISet<ActionType> permissions, ISet<Guid> partitionAccess)
    {
        ActorId = actorId;
        permissionActionTypes = permissions;
        partitionAccessGuids = partitionAccess;
    }

    public bool HasPermission(ActionType action)
    {
        return permissionActionTypes.Contains(action);
    }

    public bool HasPartitionAccess(Guid partitionGuid)
    {
        return partitionAccessGuids.Contains(partitionGuid);
    }

    public bool HasAccess(IPartition partition)
    {
        return HasPartitionAccess(partition.Guid);
    }

    public bool HasAccess(IPartitioned partitioned)
    {
        if (partitioned.Partitions.Count == 0)
        {
            return true;
        }

        return partitioned.Partitions.Any(p => partitionAccessGuids.Contains(p.Guid));
    }

    public bool HasAccess(IPartitionedGuids partitioned)
    {
        if (partitioned.PartitionGuids.Count == 0)
        {
            return true;
        }

        return partitioned.PartitionGuids.Any(partitionAccessGuids.Contains);
    }

    public void ThrowIfPermissionNotAuthorized(ActionType action)
    {
        if (!HasPermission(action))
        {
            throw new ActorPermissionDeniedException(this, action);
        }
    }

    public void ValidateHasAccess(IPartitionedGuids partitioned)
    {
        if (!HasAccess(partitioned))
        {
            throw new ActorAccessDeniedException(this, partitioned);
        }
    }

    public void ThrowIfAccessNotAuthorized(IPartitioned partitioned)
    {
        if (!HasAccess(partitioned))
        {
            throw new ActorAccessDeniedException(this, partitioned);
        }
    }

    public void ThrowIfAccessNotAuthorized(IPartition partition)
    {
        if (!HasAccess(partition))
        {
            throw new ActorAccessDeniedException(this, partition);
        }
    }
}
