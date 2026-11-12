using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Common.Actors;

public class ActorAccessDeniedFargoApplicationException : FargoApplicationException
{
    public Guid ActorGuid { get; }

    public ActorType ActorType { get; }

    public Guid EntityGuid { get; }

    public EntityType EntityType { get; }

    public ActorAccessDeniedFargoApplicationException(Guid actorGuid, ActorType actorType, Guid entityGuid, EntityType entityType)
        : base($"Access to entity '{entityGuid}' of type '{entityType}' denied for actor '{actorGuid}' of type '{actorType}'.")
    {
        ActorGuid = actorGuid;
        ActorType = actorType;
        EntityGuid = entityGuid;
        EntityType = entityType;
    }
}
