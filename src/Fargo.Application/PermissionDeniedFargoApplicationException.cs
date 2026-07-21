using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;

namespace Fargo.Application;

public class PermissionDeniedFargoApplicationException : FargoApplicationException
{
    public ActorId ActorId { get; }

    public ActionType ActionType { get; }

    public PermissionDeniedFargoApplicationException(ActorId actorId, ActionType actionType)
        : base($"Action '{actionType}' permission denied for actor '{actorId}'")
    {
        ActorId = actorId;

        ActionType = actionType;
    }
}
