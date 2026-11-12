using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.SupplyOrders;
using UnitsNet;

namespace Fargo.Application.Operations.SupplyOrders;

public sealed record SupplyOrderAddLineCommand(
    Guid SupplyOrderGuid,
    Guid ArticleGuid,
    Scalar OrderedQuantity
) : ICommand;

public sealed class SupplyOrderAddLineCommandHandler(
    ActorResolver actorService,
    ISupplyOrderRepository supplyOrderRepository,
    IArticleRepository articleRepository,
    ICurrentActor currentActor
) : ICommandHandler<SupplyOrderAddLineCommand>
{
    public async Task HandleAsync(SupplyOrderAddLineCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditSupplyOrder);

        var order = await supplyOrderRepository.GetByGuidAsync(command.SupplyOrderGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(order, command.SupplyOrderGuid);

        actor.ThrowIfAccessDenied(order);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.ArticleGuid);

        actor.ThrowIfAccessDenied(article);

        order.AddLine(article, command.OrderedQuantity);
    }
}
