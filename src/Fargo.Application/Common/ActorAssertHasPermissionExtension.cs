using Fargo.Core.Actors;
using Fargo.Core.Shared.Actions;

namespace Fargo.Application.Common;

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
