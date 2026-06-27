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
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User delete flow started for user {userGuid} by actor {actorId}.",
                command.UserGuid, currentActor.ActorId);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.DeleteUser);

        var user = await userRepository.GetByGuid(command.UserGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(user);

        userRepository.Remove(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User delete mutation completed for user {userGuid} by actor {actorId}.",
                user.Guid, actor.ActorId);
        }
    }
}
