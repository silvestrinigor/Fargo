using Fargo.Application.Identity;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fargo.Application.System;

public sealed class InitializeSystemCommandHandler(
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    IOptions<DefaultAdminOptions> defaultAdminOptions,
    IOptions<AdministratorsUserGroupOptions> administratorsOptions,
    IOptions<GlobalPartitionOptions> globalPartitionOptions,
    ILogger<InitializeSystemCommandHandler> logger
    ) : ICommandHandler<InitializeSystemCommand>
{
    public async Task HandleAsync(
        InitializeSystemCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.InitializeSystemStarted();

        if (currentActor.ActorId.ActorType != ActorType.Application)
        {
            throw new Identity.UnauthorizedAccessException();
        }

        var anyUser = await userRepository.Any(cancellationToken);

        if (anyUser)
        {
            logger.InitializeSystemSkiped();

            return;
        }

        var globalPartition = await partitionRepository.GetByGuidAsync(PartitionService.GlobalPartitionGuid, cancellationToken);

        var globalPartitionCreated = false;

        if (globalPartition is null)
        {
            globalPartition = new Partition(new(globalPartitionOptions.Value.Name))
            {
                Guid = PartitionService.GlobalPartitionGuid,
                Description = new(globalPartitionOptions.Value.Description)
            };

            partitionRepository.Add(globalPartition);

            globalPartitionCreated = true;
        }

        var administratorsGroup = await userGroupRepository.GetByGuidAsync(UserGroupService.AdministratorsUserGroupGuid, cancellationToken);

        var allActions = Enum.GetValues<ActionType>();

        var administratorsGroupCreated = false;

        if (administratorsGroup is null)
        {
            var administratorsName = new Nameid(administratorsOptions.Value.Nameid);

            administratorsGroup = new UserGroup(administratorsName)
            {
                Guid = UserGroupService.AdministratorsUserGroupGuid,
                Description = new(administratorsOptions.Value.Description)
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

        var adminNameid = new Nameid(defaultAdminOptions.Value.Nameid);

        var adminPassword = new Password(defaultAdminOptions.Value.Password);

        var passwordHash = passwordHasher.Hash(adminPassword);

        var admin = new User(adminNameid, passwordHash)
        {
            Guid = UserService.DefaultAdministratorUserGuid,
            Description = new(defaultAdminOptions.Value.Description)
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
