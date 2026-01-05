using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public class Event : IEntity
    {
        internal Event() { }

        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public DateTime OccurredAt
        {
            get;
            init;
        } = DateTime.UtcNow;

        public required Guid RelatedEntityGuid
        { 
            get;
            init;
        }

        public required EventType EventType
        {
            get;
            init;
        }

        public object? EventData
        {
            get;
            init;
        }
    }
}
