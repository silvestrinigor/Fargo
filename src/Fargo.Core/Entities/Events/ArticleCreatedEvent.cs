using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities.Events
{
    public class ArticleCreatedEvent : Event
    {
        public override EventType EventType => EventType.ArticleCreated;

        private ArticleCreatedEvent() { }

        internal ArticleCreatedEvent(Article article)
        {
            EntityGuid = article.Guid;
            ArticleName = article.Name;
        }

        public Name ArticleName
        {
            get;
            init;
        }
    }
}
