using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Items;

namespace Fargo.Application.Items;

/// <summary>
/// Command used to update an existing item.
/// </summary>
/// <param name="ItemGuid">
/// The unique identifier of the item to update.
/// </param>
/// <param name="Item">
/// The data used to update the item.
/// </param>
public sealed record ItemUpdateCommand(
        Guid ItemGuid,
        ItemUpdateModel Item
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="ItemUpdateCommand"/>.
/// </summary>
public sealed class ItemUpdateCommandHandler(
        ActorService actorService,
        IItemRepository itemRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<ItemUpdateCommand>
{
    /// <summary>
    /// Executes the command to update an existing item.
    /// </summary>
    /// <param name="command">The command containing the item identifier and update data.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved.
    /// </exception>
    /// <exception cref="ItemNotFoundFargoApplicationException">
    /// Thrown when the specified item does not exist.
    /// </exception>
    public async Task Handle(
            ItemUpdateCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditItem);

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        actor.ValidateHasAccess(item);

        if (command.Item.ProductionDate is not null)
        {
            item.ProductionDate = command.Item.ProductionDate;
        }

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishItemUpdated(item.Guid, cancellationToken);
    }
}
