using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Events;

namespace Fargo.Application.Events;

/// <summary>Query to retrieve a paged, optionally filtered list of domain events.</summary>
public sealed record EventManyQuery(
    Guid? EntityGuid = null,
    EntityType? EntityType = null,
    EventType? EventType = null,
    Guid? ActorGuid = null,
    Guid? ApiClientGuid = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    Pagination? Pagination = null
) : IQuery<IReadOnlyCollection<EventInformation>>;

/// <summary>Handles <see cref="EventManyQuery"/>.</summary>
public sealed class EventManyQueryHandler(
    ActorService actorService,
    IEventQueryRepository eventQueryRepository,
    ICurrentUser currentUser
) : IQueryHandler<EventManyQuery, IReadOnlyCollection<EventInformation>>
{
    /// <summary>Returns a paged list of domain events matching the given filters.</summary>
    public async Task<IReadOnlyCollection<EventInformation>> Handle(EventManyQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditApiClient);

        return await eventQueryRepository.GetMany(
            query.EntityGuid,
            query.EntityType,
            query.EventType,
            query.ActorGuid,
            query.ApiClientGuid,
            query.From,
            query.To,
            query.Pagination,
            cancellationToken
        );
    }
}
