using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.SupplyOrders;

namespace Fargo.Application.Commands.SupplyOrders;

public sealed record SupplyOrderCancelCommand(Guid SupplyOrderGuid) : ICommand;

public sealed class SupplyOrderCancelCommandHandler(
    ActorResolver actorService,
    ISupplyOrderRepository supplyOrderRepository,
    ICurrentActor currentActor
) : ICommandHandler<SupplyOrderCancelCommand>
{
    public async Task HandleAsync(SupplyOrderCancelCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CancelSupplyOrder);

        var order = await supplyOrderRepository.GetByGuidAsync(command.SupplyOrderGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(order, command.SupplyOrderGuid);

        actor.ThrowIfAccessDenied(order);

        order.Cancel();
    }
}
