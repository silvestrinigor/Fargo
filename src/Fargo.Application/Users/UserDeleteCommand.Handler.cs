using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Shared;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

public sealed class UserDeleteCommandHandler(
    ActorService actorService,
    IUserRepository userRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<UserDeleteCommandHandler> logger
) : ICommandHandler<UserDeleteCommand>
{
    public async Task HandleAsync(
        UserDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.UserDeleteCompleted(command.UserGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotFoundIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.DeleteUser);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(user, command.UserGuid, EntityType.User);

        actor.ThrowIfAccessDenied(user);

        userRepository.Remove(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UserDeleteCompleted(command.UserGuid, currentActor.ActorId);
    }
}
