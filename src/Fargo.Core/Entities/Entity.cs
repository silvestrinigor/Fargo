using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public abstract class Entity
    {
        public Guid Guid
        {
            get; 
            init;
        } = Guid.NewGuid();

        public abstract EntityType EntityType { get; }
    }
}
