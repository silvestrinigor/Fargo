using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Items;
using Fargo.Core.Partitions;

namespace Fargo.Application.Items;

public sealed class ItemUpdateCommandHandler(
    ItemService itemService,
    ActorResolver actorService,
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor
) : ICommandHandler<ItemUpdateCommand>
{
    // This handler is registered with the transactional unit-of-work decorator
    // (AddFargoTransactionalCommandHandler), so its whole body already runs inside
    // one explicit transaction that the decorator commits once on success. It must
    // therefore NOT open its own transaction; it simply acquires the containment
    // advisory lock on the move path, and the ambient transaction keeps that lock
    // held until the decorator commits.
    public async Task HandleAsync(ItemUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid, EntityType.Item);

        var itemAudit = AuditLog.CreateAuditLog(actor, item, AuditLogType.EntityUpdated);

        actor.ThrowIfAccessDenied(item);

        if (command.Update.ParentItemContainerGuid is { } parentItemContainerGuid && item.ParentItemContainerGuid != parentItemContainerGuid)
        {
            // Serialize concurrent moves into the same subtree so the circular-
            // containment check below (which reads other items) cannot be raced by
            // another move. The lock is transaction-scoped and is released when the
            // decorator's transaction commits or rolls back.
            await itemRepository.AcquireContainmentLockAsync(parentItemContainerGuid, cancellationToken);

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
    }
}