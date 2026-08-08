using Fargo.Core.Actors;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;

namespace Fargo.Application.Common;

public static class ActorAssertHasAccessExtension
{
    public static void ThrowIfAccessDenied<TEntity>(this Actor actor, TEntity entity)
        where TEntity : IEntity, IPartitionedReadOnly
    {
        if (!actor.HasAccess(entity))
        {
            throw new AccessDeniedFargoApplicationException(actor.Guid, actor.ActorType, entity.Guid);
        }
    }

    public static void ThrowIfAccessDenied(this Actor actor, Partition entity)
    {
        if (!actor.HasAccess(entity))
        {
            throw new AccessDeniedFargoApplicationException(actor.Guid, actor.ActorType, entity.Guid);
        }
    }
}
