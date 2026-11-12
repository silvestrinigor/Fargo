using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.SupplyOrders;
using UnitsNet;

namespace Fargo.Application.Commands.SupplyOrders;

public sealed record SupplyOrderChangeLineQuantityCommand(
    Guid SupplyOrderGuid,
    Guid ArticleGuid,
    Scalar OrderedQuantity
) : ICommand;

public sealed class SupplyOrderChangeLineQuantityCommandHandler(
    ActorResolver actorService,
    ISupplyOrderRepository supplyOrderRepository,
    ICurrentActor currentActor
) : ICommandHandler<SupplyOrderChangeLineQuantityCommand>
{
    public async Task HandleAsync(SupplyOrderChangeLineQuantityCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditSupplyOrder);

        var order = await supplyOrderRepository.GetByGuidAsync(command.SupplyOrderGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(order, command.SupplyOrderGuid);

        actor.ThrowIfAccessDenied(order);

        order.ChangeLineQuantity(command.ArticleGuid, command.OrderedQuantity);
    }
}
