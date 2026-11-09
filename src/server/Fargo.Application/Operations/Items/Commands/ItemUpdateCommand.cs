using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Operations.Items.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;


namespace Fargo.Application.Operations.Items;

/// <summary>
/// Command used to update an existing item from an API update payload.
/// </summary>
/// <remarks>
/// Implements <see cref="ITransactionalCommand"/>: the handler acquires a
/// containment advisory lock on the move path that must be held until commit, so
/// it runs inside a single explicit transaction owned by the pipeline.
/// </remarks>
public sealed record ItemUpdateCommand(
    Guid ItemGuid,
    ItemUpdateDto Update
) : ITransactionalCommand;

public sealed class ItemUpdateCommandHandler(
    ItemService itemService,
    ActorResolver actorService,
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<ItemUpdateCommand>
{
    // This handler's command implements ITransactionalCommand, so the registration
    // layer wraps it in the transactional unit-of-work decorator: its whole body runs
    // inside one explicit transaction that the decorator commits once on success. It
    // must therefore NOT open its own transaction; it simply acquires the containment
    // advisory lock on the move path, and the ambient transaction keeps that lock
    // held until the decorator commits.
    public async Task HandleAsync(ItemUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid);

        actor.ThrowIfAccessDenied(item);

        if (command.Update.ParentItemContainerGuid is { } parentItemContainerGuid && item.ParentItemContainerGuid != parentItemContainerGuid)
        {
            var parentItemContainer = await itemRepository.GetByGuidAsync(parentItemContainerGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentItemContainer, parentItemContainerGuid);

            actor.ThrowIfAccessDenied(parentItemContainer);

            // Only moving a CONTAINER item can create a circular containment
            // hierarchy: a non-container item has no descendants, so it can never
            // contain its destination. And a fixed item cannot be moved at all
            // (PlaceInsideContainer rejects it), so there is nothing to protect.
            // Restricting the lock + cycle check to the non-fixed container case
            // keeps the heavy leaf-item move path lock-free. The lock is the single
            // global containment lock, held until the decorator's transaction
            // commits, so concurrent container moves cannot race the cycle check.
            var requiresCycleProtection =
                item.Article.ArticleType == ArticleType.Container && !item.IsFixed;

            if (requiresCycleProtection)
            {
                await itemRepository.AcquireContainmentLockAsync(cancellationToken);

                await itemService.ValidateParentItemContainerHierarchyAssignmentAsync(parentItemContainer, item, cancellationToken);
            }

            await itemService.ValidateAllowedInContainerAsync(parentItemContainer, item, cancellationToken);

            item.PlaceInsideContainer(parentItemContainer);
        }
        else if (command.Update.RemoveFromParentItemContainer is true)
        {
            item.RemoveFromContainers();
        }

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
