using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Entities.Models;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Events
{
    public class ItemDeletedEvent : Event
    {
        private ItemDeletedEvent() { }

        internal ItemDeletedEvent(Item item)
        {
            ModelGuid = item.Guid;
        }

        public override EventType EventType
        {
            get;
            protected init;
        } = EventType.ItemDeleted;
    }
}
