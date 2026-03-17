using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Helpers;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserGroupCommands
{
    /// <summary>
    /// Command used to delete an existing user group.
    /// </summary>
    /// <param name="UserGroupGuid">
    /// The unique identifier of the user group to delete.
    /// </param>
    public sealed record UserGroupDeleteCommand(
            Guid UserGroupGuid
            ) : ICommand;

    /// <summary>
    /// Handles the execution of <see cref="UserGroupDeleteCommand"/>.
    /// </summary>
    public sealed class UserGroupDeleteCommandHandler(
            IUserGroupRepository userGroupRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<UserGroupDeleteCommand>
    {
        /// <summary>
        /// Executes the command to delete an existing user group.
        /// </summary>
        /// <param name="command">
        /// The command containing the user group identifier.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="UnauthorizedAccessFargoApplicationException">
        /// Thrown when the current user cannot be resolved.
        /// </exception>
        /// <exception cref="UserGroupNotFoundFargoApplicationException">
        /// Thrown when the specified user group does not exist.
        /// </exception>
        public async Task Handle(
                UserGroupDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetActiveActor(currentUser, cancellationToken);

            UserPermissionHelper.ValidatePermission(actor, ActionType.DeleteUserGroup);

            var userGroup = await userGroupRepository.GetByGuid(
                    command.UserGroupGuid,
                    cancellationToken
                    ) ?? throw new UserGroupNotFoundFargoApplicationException(command.UserGroupGuid);

            UserGroupService.ValidateUserGroupDelete(userGroup, actor);

            userGroupRepository.Remove(userGroup);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}