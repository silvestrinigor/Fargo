using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Items;

namespace Fargo.Application.Operations.Items;

/// <summary>
/// Command used to delete an item.
/// </summary>
public sealed record ItemDeleteCommand(
    Guid ItemGuid
) : ICommand;

public sealed class ItemDeleteCommandHandler(
    ActorResolver actorService,
    IItemRepository itemRepository,
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

        itemRepository.Remove(item);
    }
}
