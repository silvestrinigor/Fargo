using Fargo.Application.Common.Exceptions;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application.Common.Actors;

public class ActorNotFoundFargoApplicationException : FargoApplicationException
{
    public Guid ActorGuid { get; }

    public ActorType ActorType { get; }

    public ActorNotFoundFargoApplicationException(Guid actorGuid, ActorType actorType)
        : base($"Actor '{actorGuid}' of type '{actorType}' was not found.", FargoErrorType.EntityNotFound)
    {
        ActorGuid = actorGuid;
        ActorType = actorType;
    }

    public static void ThrowIfNull([NotNull] SecurityPrincipal? actor, Guid actorGuid, ActorType actorType)
    {
        if (actor is null)
        {
            throw new ActorNotFoundFargoApplicationException(actorGuid, actorType);
        }
    }
}
