using Fargo.Application.Exceptions;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Security;

namespace Fargo.Application.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Actor"/> to enforce
/// authorization and partition-based access control rules.
/// </summary>
/// <remarks>
/// These methods centralize validation logic used across the application layer,
/// ensuring that authorization checks are consistently applied before executing
/// domain operations.
/// </remarks>
public static class ActorExtensions
{
    extension(Actor actor)
    {
        /// <summary>
        /// Ensures that the actor has permission to execute the specified action.
        /// </summary>
        /// <param name="action">
        /// The action that requires authorization.
        /// </param>
        /// <exception cref="UserNotAuthorizedFargoApplicationException">
        /// Thrown when the actor does not have permission to perform the specified action.
        /// </exception>
        /// <remarks>
        /// This validation enforces action-level authorization rules defined by the domain.
        /// </remarks>
        public void ValidateHasPermission(ActionType action)
        {
            if (!actor.HasActionPermission(action))
            {
                throw new UserNotAuthorizedFargoApplicationException(actor.Guid, action);
            }
        }

        /// <summary>
        /// Ensures that the actor has access to the specified partition.
        /// </summary>
        /// <param name="partitionGuid">
        /// The unique identifier of the partition to validate access for.
        /// </param>
        /// <exception cref="PartitionAccessDeniedFargoApplicationException">
        /// Thrown when the actor does not have access to the specified partition.
        /// </exception>
        /// <remarks>
        /// This validation enforces partition-level isolation, ensuring that actors
        /// can only interact with data within their authorized partitions.
        /// </remarks>
        public void ValidateHasPartitionAccess(Guid partitionGuid)
        {
            if (!actor.HasPartitionAccess(partitionGuid))
            {
                throw new PartitionAccessDeniedFargoApplicationException(partitionGuid, actor.Guid);
            }
        }

        /// <summary>
        /// Ensures that the actor has access to the specified partitioned entity.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of the partitioned entity.
        /// </typeparam>
        /// <param name="partitioned">
        /// The entity whose access should be validated.
        /// </param>
        /// <exception cref="PartitionedEntityAccessDeniedFargoApplicationException">
        /// Thrown when the actor does not have access to the specified entity.
        /// </exception>
        /// <remarks>
        /// This validation combines entity-level and partition-level checks,
        /// ensuring that the actor can access the given entity based on its
        /// partition associations.
        /// </remarks>
        public void ValidateHasAccess<TEntity>(TEntity partitioned)
            where TEntity : IEntity, IPartitioned
        {
            if (!actor.HasAccess(partitioned))
            {
                throw new PartitionedEntityAccessDeniedFargoApplicationException(partitioned.Guid, actor.Guid);
            }
        }
    }
}
