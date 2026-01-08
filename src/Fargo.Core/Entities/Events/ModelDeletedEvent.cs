using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Events
{
    public class ModelDeletedEvent : Event
    {
        internal ModelDeletedEvent() { }

        internal ModelDeletedEvent(Model model) : base(model) { }

        public override EventType EventType
        {
            get;
            protected init;
        } = EventType.ModelDeleted;
    }
}
