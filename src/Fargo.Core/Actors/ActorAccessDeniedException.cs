using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

public class ActorAccessDeniedException : Exception
{
    public ActorId ActorId { get; }

    public Guid EntityGuid { get; }

    public ActorAccessDeniedException(IActor actor, IPartitionedGuids partitioned)
        : base($"Access denied for actor {actor.ActorId}")
    {
        ActorId = actor.ActorId;

        EntityGuid = partitioned.Guid;
    }

    public ActorAccessDeniedException(IActor actor, IPartitioned partitioned)
        : base($"Access denied for actor {actor.ActorId}")
    {
        ActorId = actor.ActorId;

        EntityGuid = partitioned.Guid;
    }

    public ActorAccessDeniedException(IActor actor, IPartition partition)
        : base($"Access denied for actor {actor.ActorId}")
    {
        ActorId = actor.ActorId;

        EntityGuid = partition.Guid;
    }

    public ActorAccessDeniedException(IActor actor, Guid partitionGuid)
        : base($"Access denied for actor {actor.ActorId}")
    {
        ActorId = actor.ActorId;

        EntityGuid = partitionGuid;
    }
}
