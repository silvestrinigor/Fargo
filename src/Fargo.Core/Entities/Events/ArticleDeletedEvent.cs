using Fargo.Domain.Entities.Events.Abstracts;
using Fargo.Domain.Entities.Models;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Events
{
    public class ArticleDeletedEvent : Event
    {
        private ArticleDeletedEvent() { }

        internal ArticleDeletedEvent(Article article)
        {
            ModelGuid = article.Guid;
        }

        public override EventType EventType
        {
            get;
            protected init;
        } = EventType.ArticleDeleted;
    }
}
