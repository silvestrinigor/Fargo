using Fargo.Application.Persistence.Repositories;
using Fargo.Application.Common;
using Fargo.Application.Operations.UserGroups.Dtos;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.UserGroups;

public sealed record UserGroupsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    Sort? Sort = null,
    string? Search = null
) : IQuery<PagedResult<UserGroupDto>>;

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
            query.Sort,
            query.Search,
            cancellationToken);

        return userGroups;
    }
}
