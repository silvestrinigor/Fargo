using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record EventSingleQuery(
        Guid EventGuid
        ) : IQuery<EventDto>;

    public sealed class EventSingleQueryHandler(IEventReadRepository repository) : IQueryHandlerAsync<EventSingleQuery, EventDto>
    {
        private readonly IEventReadRepository repository = repository;

        public async Task<EventDto> HandleAsync(EventSingleQuery query, CancellationToken cancellationToken = default)
        {
            var @event = await repository.GetEventByGuid(
                query.EventGuid,
                cancellationToken);

            return @event.ToDto();
        }
    }
}
