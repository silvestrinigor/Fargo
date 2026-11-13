using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Items;

namespace Fargo.Application.Commands.Items;

/// <remarks>
/// An item that is not in a container is left as it is and records no movement.
/// </remarks>
public sealed record ItemRemoveFromContainerCommand(Guid ItemGuid) : ICommand;

public sealed class ItemRemoveFromContainerCommandHandler(
    ActorResolver actorService,
    IItemRepository itemRepository,
    ICurrentActor currentActor
) : ICommandHandler<ItemRemoveFromContainerCommand>
{
    public async Task HandleAsync(ItemRemoveFromContainerCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.MoveItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid);

        actor.ThrowIfAccessDenied(item);

        if (item.ParentItemContainerGuid is null)
        {
            return;
        }

        item.RemoveFromContainers();
    }
}
