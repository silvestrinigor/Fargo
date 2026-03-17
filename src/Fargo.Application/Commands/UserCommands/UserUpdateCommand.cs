using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Helpers;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Commands.UserCommands;

/// <summary>
/// Command used to update an existing user.
/// </summary>
/// <param name="UserGuid">
/// The unique identifier of the user to update.
/// </param>
/// <param name="User">
/// The data used to update the user.
/// </param>
public sealed record UserUpdateCommand(
        Guid UserGuid,
        UserUpdateModel User
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="UserUpdateCommand"/>.
/// </summary>
public sealed class UserUpdateCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserUpdateCommand>
{
    /// <summary>
    /// Executes the command to update an existing user.
    /// </summary>
    /// <param name="command">The command containing the user identifier and update data.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved.
    /// </exception>
    /// <exception cref="UserNotFoundFargoApplicationException">
    /// Thrown when the specified user does not exist.
    /// </exception>
    public async Task Handle(
            UserUpdateCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await userRepository.GetActiveCurrentUser(currentUser, cancellationToken);

        UserPermissionHelper.ValidateHasPermission(actor, ActionType.EditUser);

        var user = await userRepository.GetByGuid(
                command.UserGuid,
                cancellationToken
                ) ?? throw new UserNotFoundFargoApplicationException(command.UserGuid);

        user.Nameid = command.User.Nameid ?? user.Nameid;
        user.FirstName = command.User.FirstName ?? user.FirstName;
        user.LastName = command.User.LastName ?? user.LastName;
        user.Description = command.User.Description ?? user.Description;

        if (command.User.DefaultPasswordExpirationPeriod is not null)
        {
            user.DefaultPasswordExpirationPeriod =
                command.User.DefaultPasswordExpirationPeriod.Value;
        }

        if (command.User.Password is not null)
        {
            UserPermissionHelper.ValidateHasPermission(actor, ActionType.ChangeOtherUserPassword);

            user.PasswordHash = passwordHasher.Hash(command.User.Password.Value);
            user.MarkPasswordChangeAsRequired();
        }

        if (command.User.Permissions is not null)
        {
            UserService.ValidateUserPermissionChange(user, actor);

            var requestedActions = command.User.Permissions
                .Select(x => x.Action)
                .Distinct()
                .ToHashSet();

            var currentActions = user.Permissions
                .Select(x => x.Action)
                .ToHashSet();

            var permissionsToAdd = requestedActions.Except(currentActions);
            var permissionsToRemove = currentActions.Except(requestedActions);

            foreach (var action in permissionsToAdd)
            {
                user.AddPermission(action);
            }

            foreach (var action in permissionsToRemove)
            {
                user.RemovePermission(action);
            }
        }

        if (command.User.IsActive is not null)
        {
            user.IsActive = command.User.IsActive.Value;
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
