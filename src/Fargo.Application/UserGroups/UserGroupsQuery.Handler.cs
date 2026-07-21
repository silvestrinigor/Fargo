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
        CancellationToken cancellationToken = default)
    {
        logger.ManyQueryStarted(currentActor.ActorId, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.ActorId);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var userGroups = await userGroupRepository.GetManyInfoAsync(
            query.WithPagination, query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions, notChildOfAnyPartition,
            cancellationToken);

        logger.ManyQueryCompleted(
            currentActor.ActorId, query.ChildOfAnyOfThesePartitions?.Count ?? 0,
            childOfAnyOfThesePartitions?.Count ?? 0, userGroups.Count);

        return userGroups;
    }
}
