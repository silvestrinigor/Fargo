using Fargo.Application.Authentication;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;
using Microsoft.Extensions.Options;

namespace Fargo.Application.System;

/// <summary>
/// Command used to initialize the system during its first startup.
/// </summary>
/// <remarks>
/// This command bootstraps the minimum required data for the system to operate.
/// It is intended to be executed only once, when no users exist in the system.
/// </remarks>
public sealed record InitializeSystemCommand() : ICommand;

/// <summary>
/// Handles <see cref="InitializeSystemCommand"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for initializing the system with its required
/// built-in entities:
/// <list type="bullet">
/// <item><description>The global partition.</description></item>
/// <item><description>The administrators user group.</description></item>
/// <item><description>The default administrator user.</description></item>
/// </list>
///
/// <para>
/// This operation is idempotent:
/// if any user already exists, the initialization is skipped.
/// </para>
///
/// <para>
/// Only the system actor is allowed to execute this command.
/// </para>
/// </remarks>
public sealed class InitializeSystemCommandHandler(
        ActorService actorService,
        IUserRepository userRepository,
        IUserGroupRepository userGroupRepository,
        IPartitionRepository partitionRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IOptions<DefaultAdminOptions> defaultAdminOptions
        )
    : ICommandHandler<InitializeSystemCommand>
{
    /// <summary>
    /// Executes the system initialization process.
    /// </summary>
    /// <param name="command">
    /// The initialization command.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not the system actor.
    /// </exception>
    /// <remarks>
    /// The initialization process performs the following steps:
    /// <list type="number">
    /// <item><description>Validates that the current actor is the system actor.</description></item>
    /// <item><description>Checks whether any user already exists in the system.</description></item>
    /// <item><description>Ensures the global partition exists.</description></item>
    /// <item><description>Ensures the administrators user group exists with full permissions.</description></item>
    /// <item><description>Creates the default administrator user.</description></item>
    /// </list>
    ///
    /// <para>
    /// The default administrator user is created using values provided through
    /// <see cref="DefaultAdminOptions"/> and is assigned:
    /// <list type="bullet">
    /// <item><description>Access to the global partition.</description></item>
    /// <item><description>Membership in the administrators group.</description></item>
    /// <item><description>All available permissions.</description></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// If the system has already been initialized (i.e., at least one user exists),
    /// the operation completes without making any changes.
    /// </para>
    /// </remarks>
    public async Task Handle(
            InitializeSystemCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (!actor.IsSystem)
        {
            throw new UnauthorizedAccessFargoApplicationException();
        }

        var anyUser = await userRepository.Any(cancellationToken);

        if (anyUser)
        {
            return;
        }

        var globalPartition = await partitionRepository.GetByGuid(PartitionService.GlobalPartitionGuid, cancellationToken);

        if (globalPartition is null)
        {
            globalPartition = new Partition
            {
                Guid = PartitionService.GlobalPartitionGuid,
                Name = new("Global")
            };

            partitionRepository.Add(globalPartition);
        }

        var administratorsGroup = await userGroupRepository.GetByGuid(UserGroupService.AdministratorsUserGroupGuid, cancellationToken);

        var actions = Enum.GetValues<ActionType>();

        if (administratorsGroup is null)
        {
            administratorsGroup = new UserGroup
            {
                Guid = UserGroupService.AdministratorsUserGroupGuid,
                Nameid = new("administrators")
            };

            administratorsGroup.AddPartitionAccess(globalPartition);

            administratorsGroup.Partitions.Add(globalPartition);

            userGroupRepository.Add(administratorsGroup);

            foreach (var a in actions)
            {
                administratorsGroup.AddPermission(a);
            }
        }

        var options = defaultAdminOptions.Value;

        var adminNameid = new Nameid(options.Nameid);
        var adminPassword = new Password(options.Password);

        var passwordHash = passwordHasher.Hash(adminPassword);

        var admin = new User
        {
            Guid = UserService.DefaultAdministratorUserGuid,
            Nameid = adminNameid,
            PasswordHash = passwordHash
        };

        admin.AddPartitionAccess(globalPartition);

        admin.UserGroups.Add(administratorsGroup);

        admin.Partitions.Add(globalPartition);

        foreach (var action in actions)
        {
            admin.AddPermission(action);
        }

        userRepository.Add(admin);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
