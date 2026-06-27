using Fargo.Application.Identity;
using Fargo.Application.Shared.Users;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;


public sealed class UsersQueryHandler(
    ActorService actorService,
    IUserQueryRepository userRepository,
    ICurrentActor currentActor,
    ILogger<UsersQueryHandler> logger
) : IQueryHandler<UsersQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> HandleAsync(
        UsersQuery query,
        CancellationToken cancellationToken = default
    )
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Users query started for actor {actorId}. Page: {page}. Limit: {limit}.",
                currentActor.ActorId, query.WithPagination.Page, query.WithPagination.Limit);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var users = await userRepository.GetManyInfo(
            query.WithPagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Users query completed for actor {actorId}. RequestedPartitionCount: {requestedPartitionCount}. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.",
                actor.ActorId, query.ChildOfAnyOfThesePartitions?.Count ?? 0, childOfAnyOfThesePartitions?.Count ?? 0, users.Count);
        }

        return users;
    }
}
