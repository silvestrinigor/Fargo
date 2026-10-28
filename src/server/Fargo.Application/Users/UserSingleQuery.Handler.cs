using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.Users;

public sealed class UserSingleQueryHandler(
    ActorResolver actorService,
    IUserQueryRepository userRepository,
    ICurrentActor currentActor
) : IQueryHandler<UserSingleQuery, UserDto?>
{
    public async Task<UserDto?> HandleAsync(
        UserSingleQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var user = await userRepository.GetInfoByGuidAsync(
            query.UserGuid,
            actor.PartitionAccessGuids,
            cancellationToken);

        return user;
    }
}
