using Fargo.Core;
using Fargo.Core.Shared.Actors;

namespace Fargo.Application;

public class ActorNotFoundFargoException : FargoException
{
    public ActorId ActorId { get; init; }

    public ActorNotFoundFargoException(ActorId actorId)
        : base($"Actor '{actorId}' was not found.")
    {
        ActorId = actorId;
    }
}
