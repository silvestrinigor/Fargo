using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Items;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemUpdateCommandHandler(
    ActorService actorService,
    IItemRepository itemRepository,
    IUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    ILogger<ItemUpdateCommandHandler> logger
) : ICommandHandler<ItemUpdateCommand>
{
    public async Task HandleAsync(
        ItemUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        var update = command.Update;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item update flow started for item {itemGuid} by actor {actorId}.",
                command.ItemGuid, currentActor.ActorId);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.EditItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(item);

        actor.ThrowIfAccessNotAuthorized(item);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item update mutation completed for item {itemGuid} by actor {actorId}.",
                item.Guid, actor.ActorId);
        }
    }
}
