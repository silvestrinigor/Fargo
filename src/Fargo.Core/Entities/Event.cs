using Fargo.Domain.Enums;
using Fargo.Domain.Events;
using Fargo.Domain.ValueObjects.EventsValueObjects;
using System.Text.Json;

namespace Fargo.Domain.Entities
{
    public class Event : IEntity
    {
        internal Event() { }

        public Event(ArticleCreatedEventArgs eventArgs) 
        {
            RelatedEntityGuid = eventArgs.Article.Guid;

            EventType = EventType.ArticleCreated;

            EventJsonData = JsonSerializer.Serialize(
                new ArticleCreatedEventData(eventArgs.Article.Name)
                );
        }

        public Event(ArticleDeletedEventArgs eventArgs)
        {
            RelatedEntityGuid = eventArgs.ArticleGuid;

            EventType = EventType.ArticleDeleted;
        }

        public Event(ItemCreatedEventArgs eventArgs)
        {
            RelatedEntityGuid = eventArgs.Item.Guid;

            EventType = EventType.ItemCreated;

            EventJsonData = JsonSerializer.Serialize(
                new ItemCreatedEventData(eventArgs.Item.ArticleGuid)
                );
        }

        public Event(ItemDeletedEventArgs eventArgs)
        {
            RelatedEntityGuid = eventArgs.ItemGuid;

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

        public Guid RelatedEntityGuid
        { 
            get;
            private init;
        }

        public EventType EventType
        {
            get;
            private init;
        }

        public string? EventJsonData
        {
            get;
            private init;
        }
    }
}
