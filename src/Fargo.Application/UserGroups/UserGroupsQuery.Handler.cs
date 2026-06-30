using Fargo.Application.Identity;
using Fargo.Application.Shared.UserGroups;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupsQueryHandler(
    ActorService actorService,
    IUserGroupQueryRepository userGroupRepository,
    ICurrentActor currentActor,
    ILogger<UserGroupsQueryHandler> logger
) : IQueryHandler<UserGroupsQuery, IReadOnlyCollection<UserGroupDto>>
{
    public async Task<IReadOnlyCollection<UserGroupDto>> HandleAsync(
        UserGroupsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User groups query started for actor {actorId}. Page: {page}. Limit: {limit}.",
                currentActor.ActorId,
                pagination.Page,
                pagination.Limit);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var userGroups = await userGroupRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User groups query completed for actor {actorID}. RequestedPartitionCount: {requestedPartitionCount}. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.",
                actor.ActorId,
                query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0,
                userGroups.Count);
        }

        return userGroups;
    }
}
