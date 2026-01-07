using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Entities.Models;
using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities.Events
{
    public class ArticleCreatedEvent : Event
    {
        private ArticleCreatedEvent() { }

        internal ArticleCreatedEvent(Article article)
        {
            ModelGuid = article.Guid;
            ArticleName = article.Name;
        }

        public override EventType EventType => EventType.ArticleCreated;

        public Name ArticleName
        {
            get;
            init;
        }
    }
}
