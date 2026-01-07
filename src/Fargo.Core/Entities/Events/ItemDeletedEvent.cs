using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Events
{
    public class ItemDeletedEvent : Event
    {
        public override EventType EventType => EventType.ItemDeleted;

        private ItemDeletedEvent() { }
        
        internal ItemDeletedEvent(Guid itemGuid)
        {
            EntityGuid = itemGuid;
        }
    }
}
