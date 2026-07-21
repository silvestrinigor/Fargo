using Fargo.Core.Entities;
using Fargo.Core.Shared;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application;

public class EntityNotFoundFargoApplicationException : FargoApplicationException
{
    public Guid EntityGuid { get; init; }

    public EntityType EntityType { get; init; }

    public EntityNotFoundFargoApplicationException(Guid entityGuid, EntityType entityType)
        : base($"Entity '{entityGuid}' of type '{entityType}' was not found.", FargoApplicationErrorType.EntityNotFound)
    {
        EntityGuid = entityGuid;

        EntityType = entityType;
    }

    public static void ThrowIfNull([NotNull] Entity? entity, Guid entityGuid, EntityType type)
    {
        if (entity is null)
        {
            throw new EntityNotFoundFargoApplicationException(entityGuid, type);
        }
    }
}
