using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.Users;

public sealed class UsersQueryHandler(
    ActorResolver actorService,
    IUserQueryRepository userRepository,
    ICurrentActor currentActor
) : IQueryHandler<UsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> HandleAsync(
        UsersQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions);

        var users = await userRepository.GetManyInfoAsync(
            query.WithPagination,
            partitionGuids,
            query.Sort,
            query.Search,
            cancellationToken);

        return users;
    }
}
