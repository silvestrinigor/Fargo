using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Application.Shared.Users;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

public sealed class UserSingleQueryHandler(
    ActorResolver actorService,
    IUserQueryRepository userRepository,
    ICurrentActor currentActor,
    ILogger<UserSingleQueryHandler> logger
) : IQueryHandler<UserSingleQuery, UserDto?>
{
    public async Task<UserDto?> HandleAsync(
        UserSingleQuery query, CancellationToken cancellationToken = default)
    {
        logger.SingleQueryStarted(query.UserGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var user = await userRepository.GetInfoByGuidAsync(
            query.UserGuid,
            actor.PartitionAccessGuids,
            cancellationToken);

        logger.SingleQueryCompleted(query.UserGuid, currentActor.Guid, user is not null);

        return user;
    }
}
