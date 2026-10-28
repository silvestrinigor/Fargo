using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupsQueryHandler(
    ActorResolver actorService,
    IUserGroupQueryRepository userGroupRepository,
    ICurrentActor currentActor
) : IQueryHandler<UserGroupsQuery, PagedResult<UserGroupDto>>
{
    public async Task<PagedResult<UserGroupDto>> HandleAsync(
        UserGroupsQuery query,
        CancellationToken cancellationToken = default)
    {
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

        return userGroups;
    }
}
