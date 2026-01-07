using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record EventAllFromEntityQuery(
        Guid? EntityGuid
        ) : IQuery<IEnumerable<EventDto>>;

    public sealed class EventAllFromEntityQueryHandler(IEventReadRepository repository) : IQueryHandlerAsync<EventAllFromEntityQuery, IEnumerable<EventDto>>
    {
        private readonly IEventReadRepository repository = repository;

        public async Task<IEnumerable<EventDto>> HandleAsync(EventAllFromEntityQuery query, CancellationToken cancellationToken = default)
        {
            var events = await repository.GetAllEventsFromModel(
                query.EntityGuid,
                cancellationToken
                );

            return events.Select(e => e.ToDto());
        }
    }
}
