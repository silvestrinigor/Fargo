using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Commands.ServiceAccounts;

public sealed record ServiceAccountSetPartitionsCommand(Guid ServiceAccountGuid, PartitionsChangeDto Change) : ICommand;

public sealed class ServiceAccountSetPartitionsCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountSetPartitionsCommand>
{
    public async Task HandleAsync(
        ServiceAccountSetPartitionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeServiceAccountPartitions);

        var plan = PartitionsChangePlan.From(command.Change);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(account.Actor);

        await plan.ApplyAsync(
            actor,
            partitionRepository,
            account.Actor.AddPartition,
            account.Actor.RemovePartition,
            account.Actor.AddPartitionAccess,
            account.Actor.RemovePartitionAccess,
            cancellationToken);
    }
}
