using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

public class AccessDeniedFargoException : FargoException
{
    public ActorId ActorId { get; }

    public Guid EntityGuid { get; }

    public EntityType EntityType { get; }

    public AccessDeniedFargoException(ActorId actorId, Guid entityGuid, EntityType entityType)
        : base($"Access to entity '{entityGuid}' of type '{entityType}' denied for actor '{actorId}'")
    {
        ActorId = actorId;

        EntityGuid = entityGuid;

        EntityType = entityType;
    }
}
