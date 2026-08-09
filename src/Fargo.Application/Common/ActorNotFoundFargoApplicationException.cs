using Fargo.Core.Actors;
using Fargo.Core.Shared.Actors;
using Fargo.Core.Shared.Common;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application.Common;

public class ActorNotFoundFargoApplicationException : FargoApplicationException
{
    public Guid ActorGuid { get; }

    public ActorType ActorType { get; }

    public ActorNotFoundFargoApplicationException(Guid actorGuid, ActorType actorType)
        : base($"Actor '{actorGuid}' was not found.", FargoErrorType.EntityNotFound)
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
