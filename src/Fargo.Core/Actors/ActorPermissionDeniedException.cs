using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

public class ActorPermissionDeniedException : Exception
{
    public ActorId ActorId { get; }

    public ActionType ActionType { get; }

    public ActorPermissionDeniedException(ActorId actorId, ActionType actionType)
        : base($"Permission to perform action '{actionType}' denied for actor '{actorId}'")
    {
        ActorId = actorId;

        ActionType = actionType;
    }
}
