using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Fargo.Application.Requests.Commands;

/// <summary>
/// Command used to initialize the system when it is first started.
///
/// This command ensures that the system has at least one user.
/// If no users exist, a default administrator account is created.
/// </summary>
public sealed record InitializeSystemCommand() : ICommand;

/// <summary>
/// Handles the execution of <see cref="InitializeSystemCommand"/>.
/// </summary>
public sealed class InitializeSystemCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IOptions<DefaultAdminOptions> defaultAdminOptions
        )
    : ICommandHandler<InitializeSystemCommand>
{
    /// <summary>
    /// Initializes the system by creating the default administrator user
    /// when no users exist in the system.
    /// </summary>
    /// <param name="command">The command requesting system initialization.</param>
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

        var options = defaultAdminOptions.Value;

        var adminNameid = new Nameid(options.Nameid);
        var adminPassword = new Password(options.Password);

        var passwordHash = passwordHasher.Hash(adminPassword);

        var admin = new User
        {
            Nameid = adminNameid,
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
