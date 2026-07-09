using Fargo.Core.Shared;

namespace Fargo.Core.Actors;

public static class ActorAssertHasPermissionExtension
{
    public static void ThrowIfPermissionDenied(this Actor actor, ActionType action)
    {
        if (!actor.HasPermission(action))
        {
            throw new ActorPermissionDeniedException(actor.ActorId, action);
        }
    }
}