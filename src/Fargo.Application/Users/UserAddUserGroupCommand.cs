using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Application.UserGroups;
using Fargo.Domain;
using Fargo.Domain.Users;

namespace Fargo.Application.Users;

/// <summary>
/// Command used to associate an existing user group with an existing user.
/// </summary>
/// <param name="UserGuid">
/// The unique identifier of the user that will receive the group membership.
/// </param>
/// <param name="UserGroupGuid">
/// The unique identifier of the user group to associate with the user.
/// </param>
public sealed record UserAddUserGroupCommand(
        Guid UserGuid,
        Guid UserGroupGuid
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="UserAddUserGroupCommand"/>.
/// </summary>
public sealed class UserAddUserGroupCommandHandler(
        ActorService actorService,
        IUserRepository userRepository,
        IUserGroupRepository userGroupRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserAddUserGroupCommand>
{
    /// <summary>
    /// Executes the command to associate a user group with a user.
    /// </summary>
    /// <param name="command">
    /// The command containing the user and user group identifiers.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated in the persistence layer.
    /// </exception>
    /// <exception cref="UserNotFoundFargoApplicationException">
    /// Thrown when the target user does not exist.
    /// </exception>
    /// <exception cref="UserGroupNotFoundFargoApplicationException">
    /// Thrown when the target user group does not exist.
    /// </exception>
    public async Task Handle(
            UserAddUserGroupCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.ChangeUserGroupMembers);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

        user.UserGroups.Add(userGroup);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
