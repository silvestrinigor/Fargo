using Fargo.Core.Actors;

namespace Fargo.Application.Common;

/// <summary>
/// Provides extension methods for verifying actor permissions in the Fargo application.
/// </summary>
public static class ActorPermissionVerificationExtension
{
    /// <summary>
    /// Throws a <see cref="ActorPermissionDeniedFargoApplicationException"/> if the specified actor does not have permission to perform the given action.
    /// This method is used to enforce permission checks within the application.
    /// </summary>
    /// <param name="actor">The actor instance to verify permissions for</param>
    /// <param name="action">The action type that permission is being checked for</param>
    /// <exception cref="ActorPermissionDeniedFargoApplicationException">
    /// Thrown when the actor does not have permission to perform the specified action.
    /// </exception>
    public static void ThrowIfPermissionDenied(this Actor actor, ActionType action)
    {
        if (!actor.HasPermission(action))
        {
            throw new ActorPermissionDeniedFargoApplicationException(actor.Guid, actor.ActorType, action);
        }
    }
}
