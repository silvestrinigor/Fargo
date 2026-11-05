using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.SupplyOrders;
using UnitsNet;

namespace Fargo.Application.Operations.SupplyOrders;

/// <summary>
/// Changes the ordered quantity of a line on a draft supply order.
/// </summary>
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
