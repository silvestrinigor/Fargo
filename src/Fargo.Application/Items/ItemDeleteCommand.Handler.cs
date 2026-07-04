using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Items;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemDeleteCommandHandler(
    ActorService actorService,
    IItemRepository itemRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<ItemDeleteCommandHandler> logger
) : ICommandHandler<ItemDeleteCommand>
{
    public async Task HandleAsync(
        ItemDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item delete flow started for item {itemGuid} by actor {actorId}.",
                command.ItemGuid, currentActor.ActorId);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.DeleteItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(item);

        itemRepository.Remove(item);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item delete mutation completed for item {itemGuid} by actor {actorId}.",
                item.Guid, actor.ActorId);
        }
    }
}
