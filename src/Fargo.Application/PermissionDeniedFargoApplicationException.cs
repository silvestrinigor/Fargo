using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;

namespace Fargo.Application;

public class PermissionDeniedFargoApplicationException : FargoApplicationException
{
    public Guid ActorGuid { get; }

    public ActorType ActorType { get; }

    public ActionType ActionType { get; }

    public PermissionDeniedFargoApplicationException(Guid actorGuid, ActorType actorType, ActionType actionType)
        : base($"Action '{actionType}' permission denied for actor '{actorGuid}'")
    {
        ActorGuid = actorGuid;

        ActorType = actorType;

        ActionType = actionType;
    }
}
