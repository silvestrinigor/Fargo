using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public class Event
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
