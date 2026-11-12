using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.SupplyOrders;

namespace Fargo.Application.Commands.SupplyOrders;

public sealed record SupplyOrderPlaceCommand(Guid SupplyOrderGuid) : ICommand;

public sealed class SupplyOrderPlaceCommandHandler(
    ActorResolver actorService,
    ISupplyOrderRepository supplyOrderRepository,
    ICurrentActor currentActor
) : ICommandHandler<SupplyOrderPlaceCommand>
{
    public async Task HandleAsync(SupplyOrderPlaceCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.PlaceSupplyOrder);

        var order = await supplyOrderRepository.GetByGuidAsync(command.SupplyOrderGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(order, command.SupplyOrderGuid);

        actor.ThrowIfAccessDenied(order);

        order.Place();
    }
}
