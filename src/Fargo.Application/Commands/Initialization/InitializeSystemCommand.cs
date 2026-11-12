using Fargo.Application.Abstractions;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Informations;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Commands.Initialization;

public sealed record InitializeSystemCommand(
    Nameid UserAdminNameid,
    Password UserAdminPassword,
    Description UserAdminDescription,
    Nameid UserGroupAdministratorsNameid,
    Description UserGroupAdministratorsDescription,
    Name GlobalPartitionName,
    Description GlobalPartitionDescription
) : ICommand;

public sealed class InitializeSystemCommandHandler(
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IPasswordHasher passwordHasher,
    ILogger<InitializeSystemCommandHandler> logger
) : ICommandHandler<InitializeSystemCommand>
{
    public async Task HandleAsync(
        InitializeSystemCommand command,
        CancellationToken cancellationToken = default)
    {
        var anyUser = await userRepository.AnyAsync(cancellationToken);

        if (anyUser)
        {
            logger.InitializeSystemSkiped();

            return;
        }

        var globalPartition = await partitionRepository.GetByGuidAsync(FargoCoreWellKnowGuids.GlobalPartitionGuid, cancellationToken);

        var globalPartitionCreated = false;

        if (globalPartition is null)
        {
            globalPartition = Partition.CreateGlobalPartition(command.GlobalPartitionName);

            globalPartition.Description = command.GlobalPartitionDescription;

            partitionRepository.Add(globalPartition);

            globalPartitionCreated = true;
        }

        var administratorsGroup = await userGroupRepository.GetByGuidAsync(FargoCoreWellKnowGuids.AdministratorsUserGroupGuid, cancellationToken);

        var allActions = Enum.GetValues<ActionType>();

        var administratorsGroupCreated = false;

        if (administratorsGroup is null)
        {
            administratorsGroup = UserGroup.CreateAdministratorsUserGroup(command.UserGroupAdministratorsNameid);

            administratorsGroup.Description = command.UserGroupAdministratorsDescription;

            foreach (var a in allActions)
            {
                administratorsGroup.AddPermission(a);
            }

            userGroupRepository.Add(administratorsGroup);

            administratorsGroupCreated = true;
        }

        var adminActor = Actor.NewAdministratorUser(command.UserAdminNameid);
        var admin = adminActor.User!;

        var passwordHash = passwordHasher.Hash(command.UserAdminPassword);

        admin.Authentication.SetPasswordHash(passwordHash);

        adminActor.Description = command.UserAdminDescription;

        foreach (var action in allActions)
        {
            adminActor.AddPermission(action);
        }

        userRepository.Add(admin);

        logger.InitializeSystemCompleted(globalPartitionCreated, administratorsGroupCreated, adminUserCreated: true);
    }
}

internal static partial class InitializeSystemCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "System initialization flow skipped because users already exist.")]
    public static partial void InitializeSystemSkiped(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "System initialization flow completed. Global partition created: {globalPartitionCreated}. Administrators group created: {administratorsUserGroupCreated}. Admin user created: {adminUserCreated}")]
    public static partial void InitializeSystemCompleted(
        this ILogger logger,
        bool globalPartitionCreated,
        bool administratorsUserGroupCreated,
        bool adminUserCreated);
}
