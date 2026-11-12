using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.SupplyOrders;

namespace Fargo.Application.Commands.SupplyOrders;

/// <summary>
/// A <see langword="null"/> container receives goods at the root.
/// </summary>
public sealed record SupplyOrderSetReceivingContainerCommand(
    Guid SupplyOrderGuid,
    Guid? ReceivingContainerGuid
) : ICommand;

public sealed class SupplyOrderSetReceivingContainerCommandHandler(
    ActorResolver actorService,
    ISupplyOrderRepository supplyOrderRepository,
    IItemRepository itemRepository,
    ICurrentActor currentActor
) : ICommandHandler<SupplyOrderSetReceivingContainerCommand>
{
    public async Task HandleAsync(SupplyOrderSetReceivingContainerCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditSupplyOrder);

        var order = await supplyOrderRepository.GetByGuidAsync(command.SupplyOrderGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(order, command.SupplyOrderGuid);

        actor.ThrowIfAccessDenied(order);

        Item? container = null;

        if (command.ReceivingContainerGuid is { } containerGuid)
        {
            container = await itemRepository.GetByGuidAsync(containerGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(container, containerGuid);

            actor.ThrowIfAccessDenied(container);
        }

        // The entity validates that the item is a container and that the order is
        // still editable (draft).
        order.SetReceivingContainer(container);
    }
}
