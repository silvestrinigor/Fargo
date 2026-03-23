using Fargo.Application.Exceptions;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Security;

namespace Fargo.Application.Extensions;

public static class ActorExtensions
{
    extension(Actor actor)
    {
        public void ValidateHassPermission(ActionType action)
        {
            if (!actor.HasActionPermission(action))
            {
                throw new UserNotAuthorizedFargoApplicationException(actor.Guid, action);
            }
        }

        public void ValidateHassPartitionAccess(Guid partitionGuid)
        {
            if (!actor.HasPartitionAccess(partitionGuid))
            {
                throw new PartitionAccessDeniedFargoApplicationException(partitionGuid, actor.Guid);
            }
        }

        public void ValidateHassAccess<TEntity>(TEntity partitioned)
            where TEntity : IEntity, IPartitioned
        {
            if (!actor.HasAccess(partitioned))
            {
                throw new PartitionedEntityAccessDeniedFargoApplicationException(partitioned.Guid, actor.Guid);
            }
        }
    }
}
