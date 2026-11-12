using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Common.Actors;

public static class ActorAccessVerificationExtension
{
    public static void ThrowIfAccessDenied<TEntity>(this SecurityPrincipal actor, TEntity entity)
        where TEntity : Entity, IPartitionedGuidsReadOnly
    {
        if (!actor.HasAccess(entity))
        {
            throw new ActorAccessDeniedFargoApplicationException(actor.Guid, actor.ActorType, entity.Guid, entity.EntityType);
        }
    }

    public static void ThrowIfAccessDenied(this SecurityPrincipal actor, Partition entity)
    {
        if (!actor.HasAccess(entity))
        {
            throw new ActorAccessDeniedFargoApplicationException(actor.Guid, actor.ActorType, entity.Guid, EntityType.Partition);
        }
    }
}
