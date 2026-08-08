using Fargo.Application.Common;
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
        logger.ManyQueryStarted(currentActor.Guid, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var userGroups = await userGroupRepository.GetManyInfoAsync(
            query.WithPagination,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken);

        logger.ManyQueryCompleted(
            currentActor.Guid,
            query.ChildOfAnyOfThesePartitions?.Count ?? 0,
            childOfAnyOfThesePartitions?.Count ?? 0,
            userGroups.Count);

        return userGroups;
    }
}
