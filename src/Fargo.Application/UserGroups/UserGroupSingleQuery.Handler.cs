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
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User group single query started for user group {userGroupGuid} by actor {actorId}.",
                query.UserGroupGuid,
                currentActor.ActorId);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var userGroup = await userGroupRepository.GetInfoByGuid(
            query.UserGroupGuid,
            query.AsOfDateTime,
            actor.PartitionAccessGuids,
            notChildOfAnyPartition: true,
            cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User group single query completed for user group {userGroupGuid} by actor {actorGuid}. Found: {found}.",
                query.UserGroupGuid,
                actor.ActorId,
                userGroup is not null);
        }

        return userGroup;
    }
}
