using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Informations;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Command to create a service account. Returns its id and one-time secret.</summary>
public sealed record ServiceAccountCreateCommand(
    ServiceAccountCreateDto Create
) : ICommand<ServiceAccountCreatedDto>;

public sealed class ServiceAccountCreateCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    IPartitionRepository partitionRepository,
    IClientSecretGenerator secretGenerator,
    IPasswordHasher passwordHasher,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountCreateCommand, ServiceAccountCreatedDto>
{
    public async Task<ServiceAccountCreatedDto> HandleAsync(ServiceAccountCreateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreateServiceAccount);

        var clientId = new Nameid(command.Create.ClientId);

        var existing = await serviceAccountRepository.GetByClientIdAsync(clientId, cancellationToken);

        if (existing is not null)
        {
            throw new ServiceAccountClientIdUnavailableFargoApplicationException(command.Create.ClientId);
        }

        var secret = secretGenerator.Generate();

        var secretHash = passwordHasher.Hash(secret);

        var account = Actor.NewServiceAccount(clientId, secretHash).ServiceAccount!;
        var accountActor = account.Actor;

        if (command.Create.Description is { } description)
        {
            accountActor.Description = new Description(description);
        }

        if (command.Create.IsActive is false)
        {
            accountActor.Deactivate();
            account.RotateAuthVersion();
        }

        if (command.Create.PermissionsToAdd is { } permissions)
        {
            foreach (var permission in permissions)
            {
                actor.ThrowIfPermissionDenied(permission);

                accountActor.AddPermission(permission);
            }
        }

        if (command.Create.PartitionsToAdd is { } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                accountActor.AddPartition(partition);
            }
        }

        if (command.Create.PartitionAccessesToAdd is { } partitionAccesses)
        {
            foreach (var partitionGuid in partitionAccesses)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                accountActor.AddPartitionAccess(partition);
            }
        }

        serviceAccountRepository.Add(account);

        return new ServiceAccountCreatedDto(accountActor.Guid, accountActor.Nameid.ToString(), secret.Value);
    }
}
