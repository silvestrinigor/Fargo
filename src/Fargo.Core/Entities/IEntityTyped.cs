using Fargo.Core.Shared;

namespace Fargo.Core.Entities;

public interface IEntityTyped : IEntity
{
    EntityType GetEntityType();
}
