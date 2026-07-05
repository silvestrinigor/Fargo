using Fargo.Application.Identity;
using Fargo.Application.Shared.Users;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

public sealed class UserSingleQueryHandler(
    ActorService actorService,
    IUserQueryRepository userRepository,
    ICurrentActor currentActor,
    ILogger<UserSingleQueryHandler> logger
) : IQueryHandler<UserSingleQuery, UserDto?>
{
    public async Task<UserDto?> HandleAsync(
        UserSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        logger.SingleQueryStarted(query.UserGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var user = await userRepository.GetInfoByGuidAsync(
            query.UserGuid,
            query.AsOfDateTime,
            actor.PartitionAccessGuids,
            notChildOfAnyPartition: true,
            cancellationToken);

        logger.SingleQueryCompleted(query.UserGuid, currentActor.ActorId, user is not null);

        return user;
    }
}
