using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Helpers;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.UserGroupCommands
{
    /// <summary>
    /// Command used to update an existing user group.
    /// </summary>
    /// <param name="UserGroupGuid">
    /// The unique identifier of the user group to update.
    /// </param>
    /// <param name="UserGroup">
    /// The data used to update the user group.
    /// </param>
    public sealed record UserGroupUpdateCommand(
            Guid UserGroupGuid,
            UserGroupUpdateModel UserGroup
            ) : ICommand;

    /// <summary>
    /// Handles the execution of <see cref="UserGroupUpdateCommand"/>.
    /// </summary>
    public sealed class UserGroupUpdateCommandHandler(
            IUserGroupRepository userGroupRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<UserGroupUpdateCommand>
    {
        /// <summary>
        /// Executes the command to update an existing user group.
        /// </summary>
        /// <param name="command">
        /// The command containing the user group identifier and update data.
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
                UserGroupUpdateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetActiveActor(currentUser, cancellationToken);

            UserPermissionHelper.ValidatePermission(actor, ActionType.EditUserGroup);

            var userGroup = await userGroupRepository.GetByGuid(
                    command.UserGroupGuid,
                    cancellationToken
                    ) ?? throw new UserGroupNotFoundFargoApplicationException(command.UserGroupGuid);

            userGroup.Nameid = command.UserGroup.Nameid ?? userGroup.Nameid;
            userGroup.Description = command.UserGroup.Description ?? userGroup.Description;

            if (command.UserGroup.Permissions is not null)
            {
                var requestedActions = command.UserGroup.Permissions
                    .Select(x => x.Action)
                    .Distinct()
                    .ToHashSet();

                var currentActions = userGroup.Permissions
                    .Select(x => x.Action)
                    .ToHashSet();

                var permissionsToAdd = requestedActions.Except(currentActions);
                var permissionsToRemove = currentActions.Except(requestedActions);

                foreach (var action in permissionsToAdd)
                {
                    userGroup.AddPermission(action);
                }

                foreach (var action in permissionsToRemove)
                {
                    userGroup.RemovePermission(action);
                }
            }

            if (command.UserGroup.IsActive is not null)
            {
                userGroup.IsActive = command.UserGroup.IsActive.Value;
            }

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}