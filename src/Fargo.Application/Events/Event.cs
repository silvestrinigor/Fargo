using Fargo.Application.Identity;
using Fargo.Core;
using Fargo.Core.Events;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Events;

#region DTOs

/// <summary>Read-model for a persisted domain event.</summary>
public sealed record EventInformation(
    Guid Guid,
    EventType EventType,
    EntityType EntityType,
    Guid EntityGuid,
    Guid ActorGuid,
    DateTimeOffset OccurredAt
);

#endregion DTOs

#region Repositories

/// <summary>Persistence contract for read-side event queries.</summary>
public interface IEventQueryRepository
{
    /// <summary>Returns a paged, optionally filtered list of events ordered by <c>OccurredAt</c> descending.</summary>
    Task<IReadOnlyCollection<EventInformation>> GetMany(
        Guid? entityGuid,
        EntityType? entityType,
        EventType? eventType,
        Guid? actorGuid,
        DateTimeOffset? from,
        DateTimeOffset? to,
        Pagination? pagination,
        CancellationToken cancellationToken = default
    );
}
/// <summary>
/// Records domain events in persistent storage as part of the current unit of work.
/// </summary>
public interface IEventRecorder
{
    Task Record(EventType eventType, EntityType entityType, Guid entityGuid, CancellationToken cancellationToken = default);
}
/// <summary>
/// Publishes domain events after entity mutations are persisted.
/// Created events are routed to partition groups so clients discover new entities.
/// Updated and Deleted events are routed to entity-specific groups so only clients
/// that have fetched the entity receive them.
/// </summary>
/// <remarks>
/// This interface is defined in the Application layer and implemented by the API layer
/// using SignalR, keeping the Application layer free of infrastructure dependencies.
/// </remarks>
public interface IFargoEventPublisher
{
    /// <summary>Publishes an event notifying clients that a new article was created.</summary>
    /// <param name="guid">The unique identifier of the created article.</param>
    /// <param name="partitionGuids">The partitions the article belongs to, used to route the event to partition-subscribed clients.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishArticleCreated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients subscribed to the article that it was updated.</summary>
    /// <param name="guid">The unique identifier of the updated article.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishArticleUpdated(Guid guid, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients subscribed to the article that it was deleted.</summary>
    /// <param name="guid">The unique identifier of the deleted article.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishArticleDeleted(Guid guid, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients that a new item was created.</summary>
    /// <param name="guid">The unique identifier of the created item.</param>
    /// <param name="articleGuid">The unique identifier of the article the item belongs to.</param>
    /// <param name="partitionGuids">The partitions the item belongs to, used to route the event to partition-subscribed clients.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishItemCreated(Guid guid, Guid articleGuid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients subscribed to the item that it was updated.</summary>
    /// <param name="guid">The unique identifier of the updated item.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishItemUpdated(Guid guid, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients subscribed to the item that it was deleted.</summary>
    /// <param name="guid">The unique identifier of the deleted item.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishItemDeleted(Guid guid, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients that a new user was created.</summary>
    /// <param name="guid">The unique identifier of the created user.</param>
    /// <param name="nameid">The NAMEID of the created user.</param>
    /// <param name="partitionGuids">The partitions the user belongs to, used to route the event to partition-subscribed clients.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishUserCreated(Guid guid, string nameid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients subscribed to the user that it was updated.</summary>
    /// <param name="guid">The unique identifier of the updated user.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishUserUpdated(Guid guid, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients subscribed to the user that it was deleted.</summary>
    /// <param name="guid">The unique identifier of the deleted user.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishUserDeleted(Guid guid, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients that a new user group was created.</summary>
    /// <param name="guid">The unique identifier of the created user group.</param>
    /// <param name="nameid">The NAMEID of the created user group.</param>
    /// <param name="partitionGuids">The partitions the user group belongs to, used to route the event to partition-subscribed clients.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishUserGroupCreated(Guid guid, string nameid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients subscribed to the user group that it was updated.</summary>
    /// <param name="guid">The unique identifier of the updated user group.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishUserGroupUpdated(Guid guid, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients subscribed to the user group that it was deleted.</summary>
    /// <param name="guid">The unique identifier of the deleted user group.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishUserGroupDeleted(Guid guid, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients that a new partition was created.</summary>
    /// <param name="guid">The unique identifier of the created partition.</param>
    /// <param name="partitionGuids">The parent partitions, used to route the event to partition-subscribed clients.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishPartitionCreated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients subscribed to the partition that it was updated.</summary>
    /// <param name="guid">The unique identifier of the updated partition.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishPartitionUpdated(Guid guid, CancellationToken ct = default);

    /// <summary>Publishes an event notifying clients subscribed to the partition that it was deleted.</summary>
    /// <param name="guid">The unique identifier of the deleted partition.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task PublishPartitionDeleted(Guid guid, CancellationToken ct = default);
}

#endregion Repositories

#region Queries

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
    IEventQueryRepository eventQueryRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<EventManyQueryHandler> logger
) : IQueryHandler<EventManyQuery, IReadOnlyCollection<EventInformation>>
{
    /// <summary>Returns a paged list of domain events matching the given filters.</summary>
    public async Task<IReadOnlyCollection<EventInformation>> Handle(EventManyQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Events query started for actor {ActorGuid}.", actor.ActorGuid);
        }

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
                actor.ActorGuid,
                query.EntityGuid,
                query.EntityType,
                query.EventType,
                events.Count);
        }

        return events;
    }
}

#endregion Queries
