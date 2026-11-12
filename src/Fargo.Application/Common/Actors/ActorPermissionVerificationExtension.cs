using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Common.Actors;

public static class ActorPermissionVerificationExtension
{
    public static void ThrowIfPermissionDenied(this SecurityPrincipal actor, ActionType action)
    {
        if (!actor.HasPermission(action))
        {
            throw new ActorPermissionDeniedFargoApplicationException(actor.Guid, actor.ActorType, action);
        }
    }
}
