using Fargo.Core;
using Fargo.Core.Entities;
using Fargo.Core.Shared;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application;

public class EntityNotFoundFargoException : FargoException
{
    public Guid EntityGuid { get; init; }

    public EntityType EntityType { get; init; }

    public EntityNotFoundFargoException(Guid entityGuid, EntityType entityType)
        : base($"Entity '{entityGuid}' of type '{entityType}' was not found.")
    {
        EntityGuid = entityGuid;

        EntityType = entityType;
    }

    public static void ThrowIfNull([NotNull] Entity? entity, Guid entityGuid, EntityType type)
    {
        if (entity is null)
        {
            throw new EntityNotFoundFargoException(entityGuid, type);
        }
    }
}
