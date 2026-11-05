using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.SupplyOrders;

namespace Fargo.Application.Operations.SupplyOrders;

/// <summary>
/// Removes a line from a draft supply order.
/// </summary>
public sealed record SupplyOrderRemoveLineCommand(
    Guid SupplyOrderGuid,
    Guid ArticleGuid
) : ICommand;

public sealed class SupplyOrderRemoveLineCommandHandler(
    ActorResolver actorService,
    ISupplyOrderRepository supplyOrderRepository,
    ICurrentActor currentActor
) : ICommandHandler<SupplyOrderRemoveLineCommand>
{
    public async Task HandleAsync(SupplyOrderRemoveLineCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditSupplyOrder);

        var order = await supplyOrderRepository.GetByGuidAsync(command.SupplyOrderGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(order, command.SupplyOrderGuid);

        actor.ThrowIfAccessDenied(order);

        order.RemoveLine(command.ArticleGuid);
    }
}
