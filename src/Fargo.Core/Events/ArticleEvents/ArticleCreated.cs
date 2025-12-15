using Fargo.Domain.Interfaces.Events;

namespace Fargo.Domain.Events.ArticleEvents
{
    public record ArticleCreated : IEntityEvent
    {
        public Guid AggregateId => throw new NotImplementedException();

        public DateTime OccurredAt => throw new NotImplementedException();
    }
}
