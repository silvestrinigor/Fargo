using Fargo.Application;
using Fargo.Core.Shared.Actions;

namespace Fargo.Core.Actors;

public static class ActorAssertHasPermissionExtension
{
    public static void ThrowIfPermissionDenied(this Actor actor, ActionType action)
    {
        if (!actor.HasPermission(action))
        {
            throw new PermissionDeniedFargoApplicationException(actor.Guid, actor.ActorType, action);
        }
    }
}
