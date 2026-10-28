using Fargo.Application.Common;
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
        using var scope = logger.BeginActorScope(currentActor);
        logger.QueryStarted("User single query", "user", query.UserGuid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var user = await userRepository.GetInfoByGuidAsync(
            query.UserGuid,
            actor.PartitionAccessGuids,
            cancellationToken);

        logger.QueryCompleted("User single query", "user", query.UserGuid, found: user is not null);

        return user;
    }
}
