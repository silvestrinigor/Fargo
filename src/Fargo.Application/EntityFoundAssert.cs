using Fargo.Core.Entities;
using Fargo.Core.Shared;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application;

public static class EntityAssertFound
{
    public static void ThrowNotFoundIfNull([NotNull] Entity? entity, Guid entityGuid, EntityType type)
    {
        if (entity is null)
        {
            throw new EntityNotFoundFargoException(entityGuid, type);
        }
    }
}
