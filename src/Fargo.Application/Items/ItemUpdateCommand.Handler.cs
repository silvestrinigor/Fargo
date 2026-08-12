using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemUpdateCommandHandler(
    ItemService itemService,
    ActorResolver actorService,
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    IItemMovimentRepository itemParentContainerHistoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    ILogger<ItemUpdateCommandHandler> logger
) : ICommandHandler<ItemUpdateCommand>
{
    public async Task HandleAsync(
        ItemUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.UpdateStarted(command.ItemGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.EditItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid, EntityType.Item);

        actor.ThrowIfAccessDenied(item);

        if (command.Update.ParentItemContainerGuid is { } parentItemContainerGuid && item.ParentItemContainerGuid != parentItemContainerGuid)
        {
            var parentItemContainer = await itemRepository.GetByGuidAsync(parentItemContainerGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentItemContainer, parentItemContainerGuid, EntityType.Item);

            actor.ThrowIfAccessDenied(parentItemContainer);

            await itemService.ValidateParentItemContainerHierarchyAssignmentAsync(parentItemContainer, item, cancellationToken);

            item.PlaceInsideContainer(parentItemContainer);

            var itemParentContainerHistory = ItemMoviment.CreateItemMoviment(item.Guid, item.ParentItemContainerGuid, DateTimeOffset.UtcNow);

            itemParentContainerHistoryRepository.Add(itemParentContainerHistory);
        }
        else if (command.Update.RemoveFromParentItemContainer is true)
        {
            item.RemoveParentItemContainer();

            var itemParentContainerHistory = ItemMoviment.CreateItemMoviment(item.Guid, item.ParentItemContainerGuid, DateTimeOffset.UtcNow);

            itemParentContainerHistoryRepository.Add(itemParentContainerHistory);
        }

        if (command.Update.PartitionsToAdd is { Count: > 0 } partitionGuidsToAdd)
        {
            foreach (var partitionGuid in partitionGuidsToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                item.AddPartition(partition);
            }
        }

        if (command.Update.PartitionsToRemove is { Count: > 0 } partitionGuidsToRemove)
        {
            foreach (var partitionGuid in partitionGuidsToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                item.RemovePartition(partition.Guid);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(item.Guid, currentActor.Guid);
    }
}
