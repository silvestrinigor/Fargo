using Fargo.Application.Persistence;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands
{
    /// <summary>
    /// Command used to initialize the system when it is first started.
    ///
    /// This command ensures that the system has at least one user.
    /// If no users exist, a default administrator account is created.
    /// </summary>
    /// <param name="DefaultAdminNameid">
    /// The NAMEID of the default administrator user.
    /// </param>
    /// <param name="DefaultAdminPassword">
    /// The password of the default administrator user.
    /// </param>
    public sealed record InitializeSystemCommand(
            Nameid? DefaultAdminNameid = null,
            Password? DefaultAdminPassword = null
            )
        : ICommand;

    /// <summary>
    /// Handles the execution of <see cref="InitializeSystemCommand"/>.
    /// </summary>
    public sealed class InitializeSystemCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork
            )
        : ICommandHandler<InitializeSystemCommand>
    {
        /// <summary>
        /// Initializes the system by creating the default administrator user
        /// when no users exist in the system.
        /// </summary>
        /// <param name="command">The command containing default admin credentials.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        public async Task Handle(
                InitializeSystemCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var anyUser = await userRepository.Any(cancellationToken);

            if (anyUser)
            {
                return;
            }

            if (command.DefaultAdminNameid == null)
            {
                throw new InvalidOperationException(
                        "Cannot create default admin user when default admin nameid is not provided.");
            }

            if (command.DefaultAdminPassword == null)
            {
                throw new InvalidOperationException(
                        "Cannot create default admin user when default admin password is not provided.");
            }

            var passwordHash = passwordHasher.Hash(command.DefaultAdminPassword.Value);

            var admin = new User
            {
                Nameid = command.DefaultAdminNameid.Value,
                PasswordHash = passwordHash
            };

            var actions = Enum.GetValues<ActionType>();

            foreach (var action in actions)
            {
                admin.AddPermission(action);
            }

            userRepository.Add(admin);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}