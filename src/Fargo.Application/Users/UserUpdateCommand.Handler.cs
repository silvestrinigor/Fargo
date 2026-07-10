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
        logger.UpdateStarted(command.UserGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotFoundIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.EditUser);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(user, command.UserGuid, EntityType.User);

        actor.ThrowIfAccessDenied(user);

        var update = command.Update;

        user.FirstName = update.FirstName ?? user.FirstName;

        user.LastName = update.LastName ?? user.LastName;

        user.Description = update.Description ?? user.Description;

        user.IsActive = update.IsActive ?? user.IsActive;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(command.UserGuid, currentActor.ActorId);
    }
}
