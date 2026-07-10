using Fargo.Core;
using Fargo.Core.Shared;

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
}
