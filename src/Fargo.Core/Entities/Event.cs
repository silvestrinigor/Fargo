using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public abstract class Event : Entity
    {
        public override EntityType EntityType => EntityType.Event;

        public abstract EventType EventType { get; }

        internal Event() { }

        public DateTime OccurredAt
        {
            get;
            init;
        } = DateTime.UtcNow;

        public Guid EntityGuid
        {
            get;
            init;
        }
    }
}
