using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.Items;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Commands.Items;

public sealed record ItemUpdateCommand(
    Guid ItemGuid,
    ItemUpdateDto Update
) : ICommand;

public sealed class ItemUpdateCommandHandler(
    ActorResolver actorService,
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<ItemUpdateCommand>
{
    public async Task HandleAsync(ItemUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid);

        actor.ThrowIfAccessDenied(item);

        if (command.Update.PartitionsToAdd is { Count: > 0 } partitionGuidsToAdd)
        {
            foreach (var partitionGuid in partitionGuidsToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                item.AddPartition(partition);
            }
        }

        if (command.Update.PartitionsToRemove is { Count: > 0 } partitionGuidsToRemove)
        {
            foreach (var partitionGuid in partitionGuidsToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                item.RemovePartition(partition.Guid);
            }
        }
    }
}
