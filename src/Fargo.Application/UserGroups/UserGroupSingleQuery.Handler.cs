using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Application.Shared.UserGroups;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupSingleQueryHandler(
    ActorResolver actorService,
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
        logger.SingleQueryStarted(query.UserGroupGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var userGroup = await userGroupRepository.GetInfoByGuidAsync(
            query.UserGroupGuid,
            actor.PartitionAccessGuids,
            notChildOfAnyPartition: true,
            cancellationToken);

        logger.SingleQueryCompleted(query.UserGroupGuid, currentActor.Guid, userGroup is not null);

        return userGroup;
    }
}
