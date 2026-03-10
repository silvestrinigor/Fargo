using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    /// <summary>
    /// Command used to create a new <see cref="User"/>.
    /// </summary>
    /// <param name="User">
    /// The data required to create the user.
    /// </param>
    public sealed record UserCreateCommand(
            UserCreateModel User
            ) : ICommand<Guid>;

    /// <summary>
    /// Handles the execution of <see cref="UserCreateCommand"/>.
    /// </summary>
    public sealed class UserCreateCommandHandler(
            UserService userService,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            IPasswordHasher passwordHasher
            ) : ICommandHandler<UserCreateCommand, Guid>
    {
        /// <summary>
        /// Executes the command to create a new user.
        /// </summary>
        /// <param name="command">The command containing the user creation data.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The unique identifier of the created user.</returns>
        /// <exception cref="UnauthorizedAccessFargoApplicationException">
        /// Thrown when the current user cannot be resolved.
        /// </exception>
        public async Task<Guid> Handle(
                UserCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            actor.ValidatePermission(ActionType.CreateUser);

            var userPasswordHash = passwordHasher.Hash(command.User.Password);

            var user = new User
            {
                Nameid = command.User.Nameid,
                Description = command.User.Description ?? Description.Empty,
                PasswordHash = userPasswordHash,
                DefaultPasswordExpirationTimeSpan =
                    command.User.DefaultPasswordExpirationTimeSpan
                    ?? TimeSpan.FromDays(User.DefaultPasswordChangeDays)
            };

            user.MarkPasswordChangeAsRequired();

            await userService.ValidateUserCreate(user, cancellationToken);

            foreach (var permission in command.User.Permissions ?? [])
            {
                user.AddPermission(permission.Action);
            }

            userRepository.Add(user);

            await unitOfWork.SaveChanges(cancellationToken);

            return user.Guid;
        }
    }
}