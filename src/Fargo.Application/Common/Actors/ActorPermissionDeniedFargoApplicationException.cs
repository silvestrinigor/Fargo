using Fargo.Application.Common.Exceptions;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Common.Actors;

public class ActorPermissionDeniedFargoApplicationException : FargoApplicationException
{
    public Guid ActorGuid { get; }

    public ActorType ActorType { get; }

    public ActionType ActionType { get; }

    public ActorPermissionDeniedFargoApplicationException(Guid actorGuid, ActorType actorType, ActionType actionType)
        : base($"Action '{actionType}' permission denied for actor '{actorGuid}' of type '{actorType}'.", FargoErrorType.PermissionDenied)
    {
        ActorGuid = actorGuid;
        ActorType = actorType;
        ActionType = actionType;
    }
}
