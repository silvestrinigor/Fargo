using Fargo.Application.Common;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

public sealed class UsersQueryHandler(
    ActorResolver actorService,
    IUserQueryRepository userRepository,
    ICurrentActor currentActor,
    ILogger<UsersQueryHandler> logger
) : IQueryHandler<UsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> HandleAsync(
        UsersQuery query, CancellationToken cancellationToken = default)
    {
        logger.ManyQueryStarted(currentActor.Guid, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions);

        var users = await userRepository.GetManyInfoAsync(
            query.WithPagination,
            partitionGuids,
            cancellationToken);

        logger.ManyQueryCompleted(
            currentActor.Guid,
            query.ChildOfAnyOfThesePartitions?.Count ?? 0,
            partitionGuids?.Count ?? 0,
            users.Items.Count);

        return users;
    }
}
