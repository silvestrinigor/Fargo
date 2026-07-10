using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

public class PermissionDeniedFargoException : FargoException
{
    public ActorId ActorId { get; }

    public ActionType ActionType { get; }

    public PermissionDeniedFargoException(ActorId actorId, ActionType actionType)
        : base($"Permission to perform action '{actionType}' denied for actor '{actorId}'")
    {
        ActorId = actorId;

        ActionType = actionType;
    }
}
