using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

public class ActorAccessDeniedException : Exception
{
    public ActorId ActorId { get; }

    public Guid EntityGuid { get; }

    public EntityType EntityType { get; }

    public ActorAccessDeniedException(ActorId actorId, Guid entityGuid, EntityType entityType)
        : base($"Access to entity '{entityGuid}' of type '{entityType}' denied for actor '{actorId}'")
    {
        ActorId = actorId;

        EntityGuid = entityGuid;

        EntityType = entityType;
    }
}
