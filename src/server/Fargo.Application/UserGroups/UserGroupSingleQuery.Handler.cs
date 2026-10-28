using Fargo.Application.Common;
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
        CancellationToken cancellationToken = default)
    {
        using var scope = logger.BeginActorScope(currentActor);
        logger.QueryStarted("User group single query", "userGroup", query.UserGroupGuid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var userGroup = await userGroupRepository.GetInfoByGuidAsync(
            query.UserGroupGuid,
            actor.PartitionAccessGuids,
            cancellationToken);

        logger.QueryCompleted("User group single query", "userGroup", query.UserGroupGuid, found: userGroup is not null);

        return userGroup;
    }
}
