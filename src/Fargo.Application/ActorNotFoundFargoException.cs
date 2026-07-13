using Fargo.Core;
using Fargo.Core.Actors;
using Fargo.Core.Shared.Actors;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application;

public class ActorNotFoundFargoException : FargoException
{
    public ActorId ActorId { get; init; }

    public ActorNotFoundFargoException(ActorId actorId)
        : base($"Actor '{actorId}' was not found.")
    {
        ActorId = actorId;
    }

    public static void ThrowIfNull([NotNull] Actor? actor, ActorId actorId)
    {
        if (actor is null)
        {
            throw new ActorNotFoundFargoException(actorId);
        }
    }
}
