using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Fargo.Application.Commands;

/// <summary>
/// Command used to initialize the system when it is first started.
///
/// When the system is empty, this command creates the built-in global
/// partition, the administrators user group, and the default
/// administrator user.
/// </summary>
public sealed record InitializeSystemCommand() : ICommand;

/// <summary>
/// Handles the execution of <see cref="InitializeSystemCommand"/>.
/// </summary>
public sealed class InitializeSystemCommandHandler(
        IUserRepository userRepository,
        IUserGroupRepository userGroupRepository,
        IPartitionRepository partitionRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IOptions<DefaultAdminOptions> defaultAdminOptions
        )
    : ICommandHandler<InitializeSystemCommand>
{
    /// <summary>
    /// Initializes the system when no users exist.
    /// </summary>
    /// <remarks>
    /// This operation bootstraps the minimum built-in data required for the
    /// system to function, including the global partition, the administrators
    /// user group, and the default administrator user.
    /// </remarks>
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

        foreach (var action in actions)
        {
            admin.AddPermission(action);
        }

        userRepository.Add(admin);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
