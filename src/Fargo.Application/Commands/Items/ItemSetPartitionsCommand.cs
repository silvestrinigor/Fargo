using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Commands.Items;

public sealed record ItemSetPartitionsCommand(Guid ItemGuid, PartitionMembershipsChangeDto Change) : ICommand;

public sealed class ItemSetPartitionsCommandHandler(
    ActorResolver actorService,
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<ItemSetPartitionsCommand>
{
    public async Task HandleAsync(
        ItemSetPartitionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeItemPartitions);

        var plan = PartitionsChangePlan.FromMemberships(command.Change);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid);

        actor.ThrowIfAccessDenied(item);

        await plan.ApplyMembershipsAsync(
            actor,
            partitionRepository,
            item.AddPartition,
            item.RemovePartition,
            cancellationToken);
    }
}
