using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Events
{
    public class ArticleDeletedEvent : Event
    {
        public override EventType EventType => EventType.ArticleDeleted;

        private ArticleDeletedEvent() { }

        internal ArticleDeletedEvent(Guid articleGuid)
        {
            EntityGuid = articleGuid;
        }
    }
}
