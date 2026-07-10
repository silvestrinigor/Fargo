using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Items;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemDeleteCommandHandler(
    ActorService actorService, IItemRepository itemRepository,
    ICurrentActor currentActor, IUnitOfWork unitOfWork,
    ILogger<ItemDeleteCommandHandler> logger
) : ICommandHandler<ItemDeleteCommand>
{
    public async Task HandleAsync(
        ItemDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.DeleteStarted(command.ItemGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotFoundIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.DeleteItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(item, command.ItemGuid, EntityType.Item);

        actor.ThrowIfAccessDenied(item);

        itemRepository.Remove(item);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.DeleteCompleted(item.Guid, currentActor.ActorId);
    }
}
