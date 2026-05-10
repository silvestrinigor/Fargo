using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Events;

/// <summary>Query to retrieve a paged, optionally filtered list of domain events.</summary>
public sealed record EventManyQuery(
    Guid? EntityGuid = null,
    EntityType? EntityType = null,
    EventType? EventType = null,
    Guid? ActorGuid = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    Pagination? Pagination = null
) : IQuery<IReadOnlyCollection<EventInformation>>;

/// <summary>Handles <see cref="EventManyQuery"/>.</summary>
public sealed class EventManyQueryHandler(
    ActorService actorService,
    IEventQueryRepository eventQueryRepository,
    ICurrentUser currentUser,
    ILogger<EventManyQueryHandler> logger
) : IQueryHandler<EventManyQuery, IReadOnlyCollection<EventInformation>>
{
    /// <summary>Returns a paged list of domain events matching the given filters.</summary>
    public async Task<IReadOnlyCollection<EventInformation>> Handle(EventManyQuery query, CancellationToken cancellationToken = default)
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Events query started for actor {ActorGuid}.", actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditUser);

        var events = await eventQueryRepository.GetMany(
            query.EntityGuid,
            query.EntityType,
            query.EventType,
            query.ActorGuid,
            query.From,
            query.To,
            query.Pagination,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Events query completed for actor {ActorGuid}. EntityGuid: {EntityGuid}. EntityType: {EntityType}. EventType: {EventType}. ResultCount: {ResultCount}.",
                actor.Guid,
                query.EntityGuid,
                query.EntityType,
                query.EventType,
                events.Count);
        }

        return events;
    }
}
