using Fargo.Core.Actors;
using Fargo.Core.Entities;

namespace Fargo.Application.Common;

public class AccessDeniedFargoApplicationException : FargoApplicationException
{
    public Guid ActorGuid { get; }

    public ActorType ActorType { get; }

    public Guid EntityGuid { get; }

    public EntityType EntityType { get; }

    public AccessDeniedFargoApplicationException(Guid actorGuid, ActorType actorType, Guid entityGuid)
        : base($"Access to entity '{entityGuid}' denied for actor '{actorGuid}'")
    {
        ActorGuid = actorGuid;

        ActorType = actorType;

        EntityGuid = entityGuid;
    }
}
