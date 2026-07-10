using Fargo.Core.Actors;
using Fargo.Core.Shared.Actors;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application;

public static class ActorAssertFound
{
    public static void ThrowNotFoundIfNull([NotNull] Actor? actor, ActorId actorId)
    {
        if (actor is null)
        {
            throw new ActorNotFoundFargoException(actorId);
        }
    }
}
