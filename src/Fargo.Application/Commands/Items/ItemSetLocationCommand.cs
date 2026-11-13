using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;

namespace Fargo.Application.Commands.Items;

/// <remarks>
/// Implements <see cref="ITransactionalCommand"/>: the handler takes a containment advisory lock on
/// the move path that must be held until commit. Placing an item in the container it is already in
/// does nothing and records no movement.
/// </remarks>
public sealed record ItemSetLocationCommand(
    Guid ItemGuid,
    Guid ParentItemContainerGuid
) : ITransactionalCommand;

public sealed class ItemSetLocationCommandHandler(
    ItemService itemService,
    ActorResolver actorService,
    IItemRepository itemRepository,
    ICurrentActor currentActor
) : ICommandHandler<ItemSetLocationCommand>
{
    // This handler's command implements ITransactionalCommand, so the registration
    // layer wraps it in the transactional unit-of-work decorator: its whole body runs
    // inside one explicit transaction that the decorator commits once on success. It
    // must therefore NOT open its own transaction; it simply acquires the containment
    // advisory lock on the move path, and the ambient transaction keeps that lock
    // held until the decorator commits.
    public async Task HandleAsync(ItemSetLocationCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.MoveItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid);

        actor.ThrowIfAccessDenied(item);

        if (item.ParentItemContainerGuid == command.ParentItemContainerGuid)
        {
            return;
        }

        var parentItemContainer = await itemRepository.GetByGuidAsync(command.ParentItemContainerGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(parentItemContainer, command.ParentItemContainerGuid);

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
}
