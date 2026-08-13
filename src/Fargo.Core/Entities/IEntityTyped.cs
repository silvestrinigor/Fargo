using Fargo.Core.Shared.Entities;

namespace Fargo.Core.Entities;

public interface IEntityTyped
{
    EntityType GetEntityType();
}
