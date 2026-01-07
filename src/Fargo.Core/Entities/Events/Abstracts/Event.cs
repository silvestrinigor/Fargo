using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Events.Abstracts
{
    public abstract class Event : IEntity
    {
        internal Event() { } 

        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public abstract EventType EventType 
        {
            get;
            protected init;
        }

        public DateTime OccurredAt
        {
            get;
            init;
        } = DateTime.UtcNow;

        public Guid ModelGuid
        {
            get;
            init;
        }
    }
}
