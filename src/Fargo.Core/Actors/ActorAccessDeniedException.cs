using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

public class ActorAccessDeniedException : Exception
{
    public ActorId ActorId { get; }

    public IPartitionedGuids Partitioned { get; }

    public ActorAccessDeniedException(IActor actor, IPartitionedGuids partitioned)
        : base($"Access denied for actor {actor.ActorId}")
    {
        ActorId = actor.ActorId;

        Partitioned = partitioned;
    }
}
