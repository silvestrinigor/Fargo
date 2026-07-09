using Fargo.Core.Shared;

namespace Fargo.Application;

public class EntityNotFoundException : Exception
{
    public Guid EntityGuid { get; init; }

    public EntityType EntityType { get; init; }

    public EntityNotFoundException(Guid entityGuid, EntityType entityType)
        : base($"Entity '{entityGuid}' of type '{entityType}' was not found.")
    {
        EntityGuid = entityGuid;

        EntityType = entityType;
    }
}
