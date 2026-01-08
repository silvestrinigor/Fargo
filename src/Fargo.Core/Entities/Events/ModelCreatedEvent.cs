using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Events
{
    public class ModelCreatedEvent : Event
    {
        internal ModelCreatedEvent() : base() { }

        internal ModelCreatedEvent(Model model) : base(model) { }

        public override EventType EventType
        {
            get;
            protected init;
        } = EventType.ModelCreated;
    }
}
