using Fargo.Application;
using Fargo.Core.Partitions;

namespace Fargo.Core.Actors;

public static class ActorAssertHasAccessExtension
{
    public static void ThrowIfAccessDenied<TEntity>(this Actor actor, TEntity entity)
        where TEntity : IPartitioned
    {
        if (!actor.HasAccess(entity))
        {
            throw new AccessDeniedFargoApplicationException(actor.ActorId, entity.Guid);
        }
    }

    public static void ThrowIfAccessDeniedToPartition<TEntity>(this Actor actor, TEntity entity)
        where TEntity : Partition
    {
        if (!actor.HasAccess(entity))
        {
            throw new AccessDeniedFargoApplicationException(actor.ActorId, entity.Guid);
        }
    }
}
