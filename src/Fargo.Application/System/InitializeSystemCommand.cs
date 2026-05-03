using Fargo.Application.ApiClients;
using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.ClientApplications;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fargo.Application.System;

public sealed record InitializeSystemCommand() : ICommand;

public sealed class InitializeSystemCommandHandler(
        ActorService actorService,
        IUserRepository userRepository,
        IUserGroupRepository userGroupRepository,
        IPartitionRepository partitionRepository,
        IApiClientRepository apiClientRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IOptions<DefaultAdminOptions> defaultAdminOptions,
        IOptions<ApiClientSeedOptions> apiClientSeedOptions,
        ILogger<InitializeSystemCommandHandler> logger
        )
    : ICommandHandler<InitializeSystemCommand>
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

        SeedApiClients(apiClientSeedOptions.Value);

        await unitOfWork.SaveChanges(cancellationToken);
    }

    private void SeedApiClients(ApiClientSeedOptions opts)
    {
        CreateApiClient(ClientApplicationService.WebApiClientGuid, "Fargo Web", opts.WebApiKey);
        CreateApiClient(ClientApplicationService.McpApiClientGuid, "Fargo MCP", opts.McpApiKey);

        if (opts.SeedTestClient)
        {
            CreateApiClient(ClientApplicationService.TestApiClientGuid, "Test", opts.TestApiKey);
        }
    }

    private void CreateApiClient(Guid guid, string name, string? configuredKey)
    {
        var plainKey = configuredKey ?? ApiKeyGenerator.Generate();
        var keyHash = ApiKeyGenerator.Hash(plainKey);

        var client = new ClientApplication
        {
            Guid = guid,
            Name = new(name),
            KeyHash = keyHash
        };

        apiClientRepository.Add(client);

        if (configuredKey is null)
        {
            logger.LogWarning("ApiClient '{Name}' created. Key (shown once): {Key}", name, plainKey);
        }
    }
}
