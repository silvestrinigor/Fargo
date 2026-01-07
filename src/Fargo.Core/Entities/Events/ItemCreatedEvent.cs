using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Events
{
    public class ItemCreatedEvent : Event
    {
        public override EventType EventType => EventType.ItemCreated;

        private ItemCreatedEvent() { }

        internal ItemCreatedEvent(Item item)
        {
            EntityGuid = item.Guid;

            ItemArticleGuid = item.ArticleGuid;
        }

        public Guid ItemArticleGuid
        {
            get;
            init;
        }
    }
}
