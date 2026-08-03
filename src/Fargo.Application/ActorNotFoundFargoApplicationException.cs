using Fargo.Core.Actors;
using Fargo.Core.Shared.Actors;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application;

public class ActorNotFoundFargoApplicationException : FargoApplicationException
{
    public Guid ActorGuid { get; }

    public ActorType ActorType { get; }

    public ActorNotFoundFargoApplicationException(Guid actorGuid, ActorType actorType)
        : base($"Actor '{actorGuid}' was not found.", FargoApplicationErrorType.ActorNotFound)
    {
        ActorGuid = actorGuid;

        ActorType = actorType;
    }

    public static void ThrowIfNull([NotNull] Actor? actor, Guid actorGuid, ActorType actorType)
    {
        if (actor is null)
        {
            throw new ActorNotFoundFargoApplicationException(actorGuid, actorType);
        }
    }
}
