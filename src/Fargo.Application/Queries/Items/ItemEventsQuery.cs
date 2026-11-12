using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.Items;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Events;

namespace Fargo.Application.Queries.Items;

public sealed record ItemEventsQuery(
    Pagination WithPagination,
    Guid? ArticleGuid = null,
    ItemEventType? EventType = null,
    DateTimeOffset? PeriodStart = null,
    DateTimeOffset? PeriodEnd = null
) : IQuery<PagedResult<ItemEventDto>>;

/// <remarks>
/// Requires <see cref="ActionType.ReadItemEvents"/>, and only returns events of articles accessible
/// through one of the actor's partitions.
/// </remarks>
public sealed class ItemEventsQueryHandler(
    ActorResolver actorResolver,
    IItemEventQueryRepository itemEventQueryRepository,
    ICurrentActor currentActor
) : IQueryHandler<ItemEventsQuery, PagedResult<ItemEventDto>>
{
    public async Task<PagedResult<ItemEventDto>> HandleAsync(ItemEventsQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorResolver.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ReadItemEvents);

        var result = await itemEventQueryRepository.GetManyInfoOrderedByOccuredAtAsync(
            query.WithPagination,
            actor.PartitionAccessGuids,
            query.ArticleGuid,
            query.EventType,
            query.PeriodStart,
            query.PeriodEnd,
            cancellationToken
        );

        return result;
    }
}
