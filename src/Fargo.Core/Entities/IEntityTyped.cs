using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public interface IEntityTyped : IEntity
    {
        EntityType EntityType { get; }
    }
}
