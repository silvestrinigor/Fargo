using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Events;
using Fargo.Domain.Users;

namespace Fargo.Application.Users;

/// <summary>
/// Command used to delete an existing user.
/// </summary>
/// <param name="UserGuid">
/// The unique identifier of the user to delete.
/// </param>
public sealed record UserDeleteCommand(
        Guid UserGuid
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="UserDeleteCommand"/>.
/// </summary>
public sealed class UserDeleteCommandHandler(
        ActorService actorService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IEventRecorder eventRecorder,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<UserDeleteCommand>
{
    /// <summary>
    /// Executes the command to delete an existing user.
    /// </summary>
    /// <param name="command">The command containing the user identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved.
    /// </exception>
    /// <exception cref="UserNotFoundFargoApplicationException">
    /// Thrown when the specified user does not exist.
    /// </exception>
    public async Task Handle(
            UserDeleteCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        UserService.ValidateUserDelete(user, actor);

        userRepository.Remove(user);

        await eventRecorder.Record(EventType.UserDeleted, EntityType.User, user.Guid, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);
        await eventPublisher.PublishUserDeleted(user.Guid, cancellationToken);
    }
}
