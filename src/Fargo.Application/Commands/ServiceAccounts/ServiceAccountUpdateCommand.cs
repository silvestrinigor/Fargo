using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.ServiceAccounts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;

namespace Fargo.Application.Commands.ServiceAccounts;

public sealed record ServiceAccountUpdateCommand(
    Guid ServiceAccountGuid,
    ServiceAccountUpdateDto Update
) : ICommand;

public sealed class ServiceAccountUpdateCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountUpdateCommand>
{
    public async Task HandleAsync(ServiceAccountUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditServiceAccount);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.Actor);

        var accountActor = account.Actor;

        if (command.Update.Description is { } description)
        {
            accountActor.Description = new Description(description);
        }

        if (command.Update.IsActive is { } isActive)
        {
            if (isActive)
            {
                accountActor.Activate();
            }
            else
            {
                accountActor.Deactivate();
                account.RotateAuthVersion();
            }
        }

        if (command.Update.PermissionsToAdd is { } permissionsToAdd)
        {
            foreach (var permission in permissionsToAdd)
            {
                actor.ThrowIfPermissionDenied(permission);

                accountActor.AddPermission(permission);
            }
        }

        if (command.Update.PermissionsToRemove is { } permissionsToRemove)
        {
            foreach (var permission in permissionsToRemove)
            {
                actor.ThrowIfPermissionDenied(permission);

                accountActor.RemovePermission(permission);
            }
        }

        if (command.Update.PartitionsToAdd is { } partitionMembershipsToAdd)
        {
            foreach (var partitionGuid in partitionMembershipsToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                accountActor.AddPartition(partition);
            }
        }

        if (command.Update.PartitionsToRemove is { } partitionMembershipsToRemove)
        {
            foreach (var partitionGuid in partitionMembershipsToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                accountActor.RemovePartition(partition.Guid);
            }
        }

        if (command.Update.PartitionAccessesToAdd is { } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                accountActor.AddPartitionAccess(partition);
            }
        }

        if (command.Update.PartitionAccessesToRemove is { } partitionAccessesToRemove)
        {
            foreach (var partitionGuid in partitionAccessesToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                accountActor.RemovePartitionAccess(partition.Guid);
            }
        }
    }
}
