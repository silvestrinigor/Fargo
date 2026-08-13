using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupsQueryHandler(
    ActorResolver actorService,
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

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions);

        var userGroups = await userGroupRepository.GetManyInfoAsync(
            query.WithPagination,
            partitionGuids,
            cancellationToken);

        logger.ManyQueryCompleted(
            currentActor.Guid,
            query.ChildOfAnyOfThesePartitions?.Count ?? 0,
            partitionGuids?.Count ?? 0,
            userGroups.Count);

        return userGroups;
    }
}
