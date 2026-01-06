using Fargo.Domain.Enums;
using Fargo.Domain.Events;
using Fargo.Domain.ValueObjects.EventsValueObjects;
using System.Text.Json;

namespace Fargo.Domain.Entities
{
    public class Event : IEntity
    {
        internal Event() { }

        public Event(ArticleCreatedEventArgs eventArgs, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            EntityGuid = eventArgs.Article.Guid;

            EventType = EventType.ArticleCreated;

            EventSerializedData = JsonSerializer.SerializeToElement(
                new ArticleCreatedEventData(eventArgs.Article.Name),
                jsonSerializerOptions
                );
        }

        public Event(ArticleDeletedEventArgs eventArgs)
        {
            EntityGuid = eventArgs.ArticleGuid;

            EventType = EventType.ArticleDeleted;
        }

        public Event(ItemCreatedEventArgs eventArgs, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            EntityGuid = eventArgs.Item.Guid;

            EventType = EventType.ItemCreated;

            EventSerializedData = JsonSerializer.SerializeToElement(
                new ItemCreatedEventData(eventArgs.Item.ArticleGuid),
                jsonSerializerOptions
                );
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

        public JsonElement? EventSerializedData
        {
            get;
            private init;
        }
    }
}
