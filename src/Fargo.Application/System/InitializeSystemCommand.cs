using Fargo.Application.Authentication;
using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fargo.Application.System;

public sealed record InitializeSystemCommand() : ICommand;

public sealed class InitializeSystemCommandHandler(
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IOptions<DefaultAdminOptions> defaultAdminOptions,
    ILogger<InitializeSystemCommandHandler> logger
    ) : ICommandHandler<InitializeSystemCommand>
{
    public async Task Handle(
        InitializeSystemCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("System initialization flow started.");
        }

        var anyUser = await userRepository.Any(cancellationToken);

        if (anyUser)
        {
            logger.LogInformation("System initialization flow skipped because users already exist.");
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
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("System initialization flow created global partition {PartitionGuid}.", globalPartition.Guid);
            }
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

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "System initialization flow created administrators user group {UserGroupGuid}. PermissionCount: {PermissionCount}.",
                    administratorsGroup.Guid,
                    administratorsGroup.Permissions.Count);
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

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "System initialization flow completed. AdminUserGuid: {UserGuid}. AdministratorsUserGroupGuid: {UserGroupGuid}.",
                admin.Guid,
                administratorsGroup.Guid);
        }
    }
}
