using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.System;

public sealed class InitializeSystemCommandHandler(
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ILogger<InitializeSystemCommandHandler> logger
    ) : ICommandHandler<InitializeSystemCommand>
{
    public async Task HandleAsync(
        InitializeSystemCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.InitializeSystemStarted();

        var anyUser = await userRepository.Any(cancellationToken);

        if (anyUser)
        {
            logger.InitializeSystemSkiped();

            return;
        }

        var globalPartition = await partitionRepository.GetByGuidAsync(FargoCoreGuids.GlobalPartitionGuid, cancellationToken);

        var globalPartitionCreated = false;

        if (globalPartition is null)
        {
            globalPartition = Partition.CreateGlobalPartition(command.GlobalPartitionName);

            globalPartition.Description = command.GlobalPartitionDescription;

            partitionRepository.Add(globalPartition);

            globalPartitionCreated = true;
        }

        var administratorsGroup = await userGroupRepository.GetByGuidAsync(FargoCoreGuids.AdminUserGroupGuid, cancellationToken);

        var allActions = Enum.GetValues<ActionType>();

        var administratorsGroupCreated = false;

        if (administratorsGroup is null)
        {
            administratorsGroup = UserGroup.CreateAdministratorsUserGroup(command.UserGroupAdministratorsNameid);

            administratorsGroup.Description = command.UserGroupAdministratorsDescription;

            administratorsGroup.AddPartitionAccess(globalPartition);

            administratorsGroup.AddPartition(globalPartition);

            foreach (var a in allActions)
            {
                administratorsGroup.AddPermission(a);
            }

            userGroupRepository.Add(administratorsGroup);

            administratorsGroupCreated = true;
        }

        var passwordHash = passwordHasher.Hash(command.UserAdminPassword);

        var admin = User.CreateAdministratorUser(command.UserAdminNameid, new(passwordHash));

        admin.Description = command.UserAdminDescription;

        admin.AddPartitionAccess(globalPartition);

        admin.AddUserGroup(administratorsGroup);

        admin.AddPartition(globalPartition);

        foreach (var action in allActions)
        {
            admin.AddPermission(action);
        }

        userRepository.Add(admin);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.InitializeSystemCompleted(globalPartitionCreated, administratorsGroupCreated, true);
    }
}
