using Fargo.Application.Persistence;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands
{
    public sealed record InitializeSystemCommand(
            Nameid? DefaultAdminNameid = null,
            Password? DefaultAdminPassword = null
            )
        : ICommand;

    public sealed class InitializeSystemCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork
            )
        : ICommandHandler<InitializeSystemCommand>
    {
        public async Task Handle(
                InitializeSystemCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var anyUser = await userRepository.Any(cancellationToken);

            if (anyUser) return;

            if (command.DefaultAdminNameid == null)
                throw new InvalidOperationException(
                        "Cannot create default admin user when default admin nameid is not provided.");

            if (command.DefaultAdminPassword == null)
                throw new InvalidOperationException(
                        "Cannot create default admin user when default admin password is not provided.");

            var passwordHash = passwordHasher.Hash(new Password(command.DefaultAdminPassword));

            var admin = new User
            {
                Nameid = command.DefaultAdminNameid.Value,
                PasswordHash = passwordHash
            };

            var actions = Enum.GetValues<ActionType>().ToList();

            foreach (ActionType a in actions)
            {
                admin.AddPermission(a);
            }

            userRepository.Add(admin);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}