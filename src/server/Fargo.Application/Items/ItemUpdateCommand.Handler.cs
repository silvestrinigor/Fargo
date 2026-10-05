using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemUpdateCommandHandler(
    ItemService itemService,
    ActorResolver actorService,
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    IUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    ILogger<ItemUpdateCommandHandler> logger
) : ICommandHandler<ItemUpdateCommand>
{
    public async Task HandleAsync(ItemUpdateCommand command, CancellationToken cancellationToken = default)
    {
        logger.UpdateStarted(command.ItemGuid, currentActor.Guid, currentActor.ActorType);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.EditItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid, EntityType.Item);

        var itemAudit = AuditLog.CreateAuditLog(actor, item, ActionType.EditItem);

        actor.ThrowIfAccessDenied(item);

        if (command.Update.ParentItemContainerGuid is { } parentItemContainerGuid && item.ParentItemContainerGuid != parentItemContainerGuid)
        {
            var parentItemContainer = await itemRepository.GetByGuidAsync(parentItemContainerGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentItemContainer, parentItemContainerGuid, EntityType.Item);

            actor.ThrowIfAccessDenied(parentItemContainer);

            await itemService.ValidateParentItemContainerHierarchyAssignmentAsync(parentItemContainer, item, cancellationToken);

            item.PlaceInsideContainer(parentItemContainer);

            itemAudit.Metadata.AddParentContainer(item.ParentItemContainerGuid);
        }
        else if (command.Update.RemoveFromParentItemContainer is true)
        {
            item.RemoveFromContainers();

            itemAudit.Metadata.AddParentContainer(null);
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

            itemAudit.Metadata.AddPartitionsAdded(partitionGuidsToAdd);
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

            itemAudit.Metadata.AddPartitionsRemoved(partitionGuidsToRemove);
        }

        auditLogRepository.Add(itemAudit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(item.Guid, currentActor.Guid, currentActor.ActorType);
    }
}
