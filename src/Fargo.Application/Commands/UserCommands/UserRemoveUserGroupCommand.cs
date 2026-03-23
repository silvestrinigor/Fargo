using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.UserCommands;

/// <summary>
/// Command used to remove an existing user group membership from a user.
/// </summary>
/// <param name="UserGuid">
/// The unique identifier of the user whose group membership will be removed.
/// </param>
/// <param name="UserGroupGuid">
/// The unique identifier of the user group to remove from the user.
/// </param>
public sealed record UserRemoveUserGroupCommand(
        Guid UserGuid,
        Guid UserGroupGuid
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="UserRemoveUserGroupCommand"/>.
/// </summary>
public sealed class UserRemoveUserGroupCommandHandler(
        ActorService actorService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserRemoveUserGroupCommand>
{
    /// <summary>
    /// Executes the command to remove a user group membership from a user.
    /// </summary>
    /// <param name="command">
    /// The command containing the user and user group identifiers.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor cannot be resolved.
    /// </exception>
    /// <exception cref="UserNotFoundFargoApplicationException">
    /// Thrown when the target user does not exist.
    /// </exception>
    public async Task Handle(
            UserRemoveUserGroupCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedUserActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHassPermission(ActionType.ChangeUserGroupMembers);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        var userGroupToRemove = user.UserGroups.FirstOrDefault(g => g.Guid == command.UserGroupGuid);

        if (userGroupToRemove != null)
        {
            user.UserGroups.Remove(userGroupToRemove);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
