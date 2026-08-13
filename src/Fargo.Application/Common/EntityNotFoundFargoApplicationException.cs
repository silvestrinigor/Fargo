using Fargo.Core.Common;
using Fargo.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application.Common;

public class EntityNotFoundFargoApplicationException : FargoApplicationException
{
    public Guid EntityGuid { get; init; }

    public EntityType EntityType { get; init; }

    public EntityNotFoundFargoApplicationException(Guid entityGuid, EntityType entityType)
        : base($"Entity '{entityGuid}' of type '{entityType}' was not found.", FargoErrorType.EntityNotFound)
    {
        EntityGuid = entityGuid;

        EntityType = entityType;
    }

    public static void ThrowIfNull([NotNull] IEntity? entity, Guid entityGuid, EntityType type)
    {
        if (entity is null)
        {
            throw new EntityNotFoundFargoApplicationException(entityGuid, type);
        }
    }
}
