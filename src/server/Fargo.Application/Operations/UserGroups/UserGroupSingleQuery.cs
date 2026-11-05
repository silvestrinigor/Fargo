using Fargo.Application.Common;
using Fargo.Application.Operations.UserGroups.Dtos;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.UserGroups;

public sealed record UserGroupSingleQuery(
    Guid UserGroupGuid
) : IQuery<UserGroupDto?>;

public sealed class UserGroupSingleQueryHandler(
    ActorResolver actorService,
    IUserGroupQueryRepository userGroupRepository,
    ICurrentActor currentActor
) : IQueryHandler<UserGroupSingleQuery, UserGroupDto?>
{
    public async Task<UserGroupDto?> HandleAsync(
        UserGroupSingleQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var userGroup = await userGroupRepository.GetInfoByGuidAsync(
            query.UserGroupGuid,
            actor.PartitionAccessGuids,
            cancellationToken);

        return userGroup;
    }
}
