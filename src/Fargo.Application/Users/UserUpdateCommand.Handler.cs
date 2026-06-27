using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Shared;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

public sealed class UserUpdateCommandHandler(
    ActorService actorService,
    IUserRepository userRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<UserUpdateCommandHandler> logger
) : ICommandHandler<UserUpdateCommand>
{
    public async Task HandleAsync(
        UserUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User update flow started for user {userGuid} by actor {actorId}.",
                command.UserGuid, currentActor.ActorId);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.EditUser);

        var user = await userRepository.GetByGuid(command.UserGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(user);

        var update = command.Update;

        user.FirstName = update.FirstName ?? user.FirstName;

        user.LastName = update.LastName ?? user.LastName;

        user.Description = update.Description ?? user.Description;

        user.IsActive = update.IsActive ?? user.IsActive;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User update mutation completed for user {userGuid} by actor {actorId}.",
                user.Guid, actor.ActorId);
        }
    }
}
