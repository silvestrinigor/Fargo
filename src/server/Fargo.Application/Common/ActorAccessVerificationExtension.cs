using Fargo.Core.Actors;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;

namespace Fargo.Application.Common;

/// <summary>
/// Provides extension methods for verifying actor access to entities in the Fargo application.
/// </summary>
public static class ActorAccessVerificationExtension
{
    /// <summary>
    /// Throws a <see cref="ActorAccessDeniedFargoApplicationException"/> if the specified actor does not have access to the given entity.
    /// This method is used to enforce access control checks for typed entities that implement <see cref="IEntityTyped"/> and <see cref="IPartitionedGuidsReadOnly"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to verify access for, must implement <see cref="IEntity"/>, <see cref="IEntityTyped"/>, and <see cref="IPartitionedGuidsReadOnly"/></typeparam>
    /// <param name="actor">The actor instance to verify access for</param>
    /// <param name="entity">The entity that access is being checked for</param>
    /// <exception cref="ActorAccessDeniedFargoApplicationException">
    /// Thrown when the actor does not have access to the specified entity.
    /// </exception>
    public static void ThrowIfAccessDenied<TEntity>(this Actor actor, TEntity entity)
        where TEntity : IEntity, IEntityTyped, IPartitionedGuidsReadOnly
    {
        if (!actor.HasAccess(entity))
        {
            throw new ActorAccessDeniedFargoApplicationException(actor.Guid, actor.ActorType, entity.Guid, entity.GetEntityType());
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
    public static void ThrowIfAccessDenied(this Actor actor, Partition entity)
    {
        if (!actor.HasAccess(entity))
        {
            throw new ActorAccessDeniedFargoApplicationException(actor.Guid, actor.ActorType, entity.Guid, EntityType.Partition);
        }
    }
}
