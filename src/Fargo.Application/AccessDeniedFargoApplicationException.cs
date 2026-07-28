using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;

namespace Fargo.Application;

public class AccessDeniedFargoApplicationException : FargoApplicationException
{
    public ActorId ActorId { get; }

    public Guid EntityGuid { get; }

    public EntityType EntityType { get; }

    public AccessDeniedFargoApplicationException(ActorId actorId, Guid entityGuid)
        : base($"Access to entity '{entityGuid}' denied for actor '{actorId}'")
    {
        ActorId = actorId;

        EntityGuid = entityGuid;
    }
}
