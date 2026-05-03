using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;
using Microsoft.Extensions.Options;

namespace Fargo.Application.System;

public sealed record InitializeSystemCommand() : ICommand;

public sealed class InitializeSystemCommandHandler(
    ActorService actorService,
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IOptions<DefaultAdminOptions> defaultAdminOptions
    ) : ICommandHandler<InitializeSystemCommand>
{
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
