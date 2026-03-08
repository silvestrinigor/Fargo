using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;

namespace Fargo.Application.Requests.Commands.UserCommands
{
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
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            actor.ValidatePermission(ActionType.EditUser);

            var user = await userRepository.GetByGuid(
                    command.UserGuid,
                    cancellationToken
                    ) ?? throw new UserNotFoundFargoApplicationException(command.UserGuid);

            user.Nameid = command.User.Nameid ?? user.Nameid;

            user.Description = command.User.Description ?? user.Description;

            if (command.User.Password != null)
            {
                var userPasswordHash = passwordHasher.Hash(command.User.Password.NewPassword);

                user.PasswordHash = userPasswordHash;
            }

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}