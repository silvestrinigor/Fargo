using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Helpers;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.UserCommands
{
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
            var actor = await userRepository.GetActiveActor(currentUser, cancellationToken);

            UserPermissionHelper.ValidatePermission(actor, ActionType.ChangeUserGroupMembers);

            var user = await userRepository.GetByGuid(
                    command.UserGuid,
                    cancellationToken
                    )
                ?? throw new UserNotFoundFargoApplicationException(command.UserGuid);
            var userGroup = await userGroupRepository.GetByGuid(
                    command.UserGroupGuid,
                    cancellationToken
                    )
                ?? throw new UserGroupNotFoundFargoApplicationException(command.UserGroupGuid);

            user.UserGroups.Add(userGroup);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}