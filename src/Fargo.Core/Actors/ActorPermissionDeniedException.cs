using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

public class ActorPermissionDeniedException : Exception
{
    public ActorId ActorId { get; }

    public ActionType ActionType { get; }

    public ActorPermissionDeniedException(IActor actor, ActionType actionType)
        : base($"Permission denied for actor {actor.ActorId}")
    {
        ActorId = actor.ActorId;

        ActionType = actionType;
    }
}
