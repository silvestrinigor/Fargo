using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Entities.Models;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Events
{
    public class ItemCreatedEvent : Event
    {
        private ItemCreatedEvent() { }

        internal ItemCreatedEvent(Item item)
        {
            ModelGuid = item.Guid;

            ItemArticleGuid = item.ArticleGuid;
        }

        public override EventType EventType
        {
            get;
            protected init;
        } = EventType.ItemCreated;

        public Guid ItemArticleGuid
        {
            get;
            init;
        }
    }
}
