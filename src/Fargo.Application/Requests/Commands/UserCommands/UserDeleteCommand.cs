using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserCommands
{
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
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
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
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            actor.ValidateIsActive();
            actor.ValidatePermission(ActionType.DeleteUser);

            var user = await userRepository.GetByGuid(
                    command.UserGuid,
                    cancellationToken
                    ) ?? throw new UserNotFoundFargoApplicationException(command.UserGuid);

            UserService.ValidateUserDelete(user, actor);

            userRepository.Remove(user);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}