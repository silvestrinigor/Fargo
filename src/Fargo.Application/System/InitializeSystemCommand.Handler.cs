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

        var globalPartition = await partitionRepository.GetByGuidAsync(FargoConstantGuids.GlobalPartitionGuid, cancellationToken);

        var globalPartitionCreated = false;

        if (globalPartition is null)
        {
            globalPartition = new Partition(command.GlobalPartitionName)
            {
                Guid = FargoConstantGuids.GlobalPartitionGuid,
                Description = command.GlobalPartitionDescription
            };

            partitionRepository.Add(globalPartition);

            globalPartitionCreated = true;
        }

        var administratorsGroup = await userGroupRepository.GetByGuidAsync(FargoConstantGuids.AdminUserGroupGuid, cancellationToken);

        var allActions = Enum.GetValues<ActionType>();

        var administratorsGroupCreated = false;

        if (administratorsGroup is null)
        {
            administratorsGroup = new UserGroup(command.UserGroupAdministratorsNameid)
            {
                Guid = FargoConstantGuids.AdminUserGroupGuid,
                Description = command.UserGroupAdministratorsDescription
            };

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

        var admin = new User(command.UserAdminNameid, passwordHash)
        {
            Guid = FargoConstantGuids.AdminUserGuid,
            Description = command.UserAdminDescription
        };

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
