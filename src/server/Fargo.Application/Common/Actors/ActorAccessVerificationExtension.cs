using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Common.Actors;

/// <summary>
/// Provides extension methods for verifying actor access to entities in the Fargo application.
/// </summary>
public static class ActorAccessVerificationExtension
{
    /// <summary>
    /// Throws a <see cref="ActorAccessDeniedFargoApplicationException"/> if the specified actor does not have access to the given entity.
    /// This method is used to enforce access control checks for entities that derive from <see cref="Entity"/> and implement <see cref="IPartitionedGuidsReadOnly"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to verify access for, must derive from <see cref="Entity"/> and implement <see cref="IPartitionedGuidsReadOnly"/></typeparam>
    /// <param name="actor">The actor instance to verify access for</param>
    /// <param name="entity">The entity that access is being checked for</param>
    /// <exception cref="ActorAccessDeniedFargoApplicationException">
    /// Thrown when the actor does not have access to the specified entity.
    /// </exception>
    public static void ThrowIfAccessDenied<TEntity>(this SecurityPrincipal actor, TEntity entity)
        where TEntity : Entity, IPartitionedGuidsReadOnly
    {
        if (!actor.HasAccess(entity))
        {
            throw new ActorAccessDeniedFargoApplicationException(actor.Guid, actor.ActorType, entity.Guid, entity.EntityType);
        }
    }

    /// <summary>
    /// Throws a <see cref="ActorAccessDeniedFargoApplicationException"/> if the specified actor does not have access to the given partition.
    /// This method is used to enforce access control checks for partition entities.
    /// </summary>
    /// <param name="actor">The actor instance to verify access for</param>
    /// <param name="entity">The partition that access is being checked for</param>
    /// <exception cref="ActorAccessDeniedFargoApplicationException">
    /// Thrown when the actor does not have access to the specified partition.
    /// </exception>
    public static void ThrowIfAccessDenied(this SecurityPrincipal actor, Partition entity)
    {
        if (!actor.HasAccess(entity))
        {
            throw new ActorAccessDeniedFargoApplicationException(actor.Guid, actor.ActorType, entity.Guid, EntityType.Partition);
        }
    }
}
