using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Items;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Entities;
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
        logger.UpdateStarted(command.ItemGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.EditItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid, EntityType.Item);

        actor.ThrowIfAccessDenied(item);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(item.Guid, currentActor.Guid);
    }
}
