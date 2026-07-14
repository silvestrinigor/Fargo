using Fargo.Core.Entities;
using Fargo.Core.Partitions;

namespace Fargo.Core.Actors;

public static class ActorAssertHasAccessExtension
{
    public static void ThrowIfAccessDenied<TEntity>(this Actor actor, TEntity entity)
        where TEntity : IPartitioned, IEntityTyped
    {
        if (!actor.HasAccess(entity))
        {
            throw new AccessDeniedFargoException(actor.ActorId, entity.Guid, entity.GetEntityType());
        }
    }

    public static void ThrowIfAccessDeniedToPartition<TEntity>(this Actor actor, TEntity entity)
        where TEntity : IPartition, IEntityTyped
    {
        if (!actor.HasAccess(entity))
        {
            throw new AccessDeniedFargoException(actor.ActorId, entity.Guid, entity.GetEntityType());
        }
    }
}
