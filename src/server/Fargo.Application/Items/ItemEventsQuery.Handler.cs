using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.Items;

/// <summary>
/// Handles queries that retrieve a paginated collection of item events
/// accessible to the current actor.
/// </summary>
/// <remarks>
/// Reading item events requires the <see cref="ActionType.ReadItemEvents"/>
/// permission. Results are further scoped to events whose article is accessible
/// through one of the actor's partitions, so an actor only ever sees events for
/// articles it has access to.
/// </remarks>
/// <param name="actorResolver">Resolves the current actor and its partition access.</param>
/// <param name="itemEventQueryRepository">Provides access to item event query data.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
public sealed class ItemEventsQueryHandler(
    ActorResolver actorResolver,
    IItemEventQueryRepository itemEventQueryRepository,
    ICurrentActor currentActor
) : IQueryHandler<ItemEventsQuery, PagedResult<ItemEventDto>>
{
    /// <summary>
    /// Handles the <see cref="ItemEventsQuery"/> by retrieving a paginated
    /// collection of item events accessible to the current actor.
    /// </summary>
    /// <param name="query">The query carrying pagination and filter criteria.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A page of item event DTOs together with the total count.</returns>
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
