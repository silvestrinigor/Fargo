using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Items;
using Fargo.Core.Events;

namespace Fargo.Application.Commands.Items;

public sealed record ItemDeleteCommand(
    Guid ItemGuid
) : ICommand;

public sealed class ItemDeleteCommandHandler(
    ActorResolver actorService,
    IItemRepository itemRepository,
    IItemEventRepository itemEventRepository,
    ICurrentActor currentActor
) : ICommandHandler<ItemDeleteCommand>
{
    public async Task HandleAsync(
        ItemDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeleteItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid);

        actor.ThrowIfAccessDenied(item);

        // Deleting a container deletes the items inside it in the database, so each one needs its own event.
        var containedItems = await itemRepository.GetContainedItemsAsync(item.Guid, cancellationToken);

        foreach (var containedItem in containedItems)
        {
            itemEventRepository.Add(ItemEvent.NewContainedItemDeletedEvent(containedItem.ItemGuid, containedItem.ArticleGuid));
        }

        itemEventRepository.Add(ItemEvent.NewItemDeletedEvent(item));

        itemRepository.Remove(item);
    }
}
