using Fargo.Domain.Enums;
using Fargo.Domain.Events;
using Fargo.Domain.ValueObjects.EventsValueObjects;

namespace Fargo.Domain.Entities
{
    public class Event : IEntity
    {
        internal Event() { }

        public Event(ArticleCreatedEventArgs eventArgs)
        {
            EntityGuid = eventArgs.Article.Guid;

            EventType = EventType.ArticleCreated;

            EventData = new ArticleCreatedEventData(eventArgs.Article.Name);
        }

        public Event(ArticleDeletedEventArgs eventArgs)
        {
            EntityGuid = eventArgs.ArticleGuid;

            EventType = EventType.ArticleDeleted;
        }

        public Event(ItemCreatedEventArgs eventArgs)
        {
            EntityGuid = eventArgs.Item.Guid;

            EventType = EventType.ItemCreated;

            EventData = new ItemCreatedEventData(eventArgs.Item.ArticleGuid);
        }

        public Event(ItemDeletedEventArgs eventArgs)
        {
            EntityGuid = eventArgs.ItemGuid;

            EventType = EventType.ItemDeleted;
        }

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

        public Guid EntityGuid
        { 
            get;
            private init;
        }

        public EventType EventType
        {
            get;
            private init;
        }

        public IEventData? EventData
        {
            get;
            private init;
        }
    }
}
