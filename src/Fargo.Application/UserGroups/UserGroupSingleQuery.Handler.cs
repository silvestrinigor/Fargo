using Fargo.Application.Identity;
using Fargo.Application.Shared.UserGroups;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupSingleQueryHandler(
    ActorService actorService,
    IUserGroupQueryRepository userGroupRepository,
    ICurrentActor currentActor,
    ILogger<UserGroupSingleQueryHandler> logger
) : IQueryHandler<UserGroupSingleQuery, UserGroupDto?>
{
    public async Task<UserGroupDto?> HandleAsync(
        UserGroupSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        logger.SingleQueryStarted(query.UserGroupGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.ActorId);

        var userGroup = await userGroupRepository.GetInfoByGuidAsync(
            query.UserGroupGuid,
            query.AsOfDateTime,
            actor.PartitionAccessGuids,
            notChildOfAnyPartition: true,
            cancellationToken);

        logger.SingleQueryCompleted(query.UserGroupGuid, currentActor.ActorId, userGroup is not null);

        return userGroup;
    }
}
