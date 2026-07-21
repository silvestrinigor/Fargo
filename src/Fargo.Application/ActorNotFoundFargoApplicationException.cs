using Fargo.Core.Actors;
using Fargo.Core.Shared.Actors;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application;

public class ActorNotFoundFargoApplicationException : FargoApplicationException
{
    public ActorId ActorId { get; init; }

    public ActorNotFoundFargoApplicationException(ActorId actorId)
        : base($"Actor '{actorId}' was not found.", FargoApplicationErrorType.ActorNotFound)
    {
        ActorId = actorId;
    }

    public static void ThrowIfNull([NotNull] Actor? actor, ActorId actorId)
    {
        if (actor is null)
        {
            throw new ActorNotFoundFargoApplicationException(actorId);
        }
    }
}
