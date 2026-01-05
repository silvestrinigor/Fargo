using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public class Event : IEntityTyped
    {
        internal Event() 
        { 
            EntityType = EntityType.Event;
        }

        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public EntityType EntityType 
        { 
            get;
        }

        public DateTime OccurredAt
        {
            get;
            init;
        } = DateTime.UtcNow;

        public required Guid EntityGuid
        {
            get;
            init;
        }

        public required EventType EventType
        {
            get;
            init;
        }
    }
}
