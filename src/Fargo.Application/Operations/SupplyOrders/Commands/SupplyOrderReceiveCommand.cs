using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Operations.SupplyOrders.Dtos;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.SupplyOrders;
using Fargo.Core.Events;
using UnitsNet;

namespace Fargo.Application.Operations.SupplyOrders;

/// <summary>
/// Receives goods against a supply order, effecting the stock change and
/// advancing the order's received quantities and status.
/// </summary>
/// <remarks>
/// Runs as an <see cref="ITransactionalCommand"/>: the stock change and the
/// order update commit atomically. Concurrency against a racing receive on the
/// same order is handled by the order's optimistic concurrency token (a racing
/// receive fails with a concurrency exception rather than double-counting).
///
/// For an entity-tracked article each received unit materializes one
/// <see cref="Item"/> placed into the order's receiving container (or at the
/// root when the order has none); the received quantity must therefore be a
/// whole number. For an event-tracked article a single
/// <see cref="ItemEventType.Creation"/> event records the received quantity,
/// which may be fractional.
/// </remarks>
public sealed record SupplyOrderReceiveCommand(
    Guid SupplyOrderGuid,
    SupplyOrderReceiveDto Receive
) : ITransactionalCommand;

public sealed class SupplyOrderReceiveCommandHandler(
    ActorResolver actorService,
    ISupplyOrderRepository supplyOrderRepository,
    IItemRepository itemRepository,
    IItemEventRepository itemEventRepository,
    ICurrentActor currentActor
) : ICommandHandler<SupplyOrderReceiveCommand>
{
    public async Task HandleAsync(SupplyOrderReceiveCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ReceiveSupplyOrder);

        var order = await supplyOrderRepository.GetByGuidAsync(command.SupplyOrderGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(order, command.SupplyOrderGuid);

        actor.ThrowIfAccessDenied(order);

        if (command.Receive.Lines is not { Count: > 0 } receiveLines)
        {
            throw new FargoApplicationException(
                "A receive operation must include at least one line.",
                FargoErrorType.InvalidOperation);
        }

        // The destination is fixed on the order (null = received at the root).
        var container = order.ReceivingContainer;

        foreach (var receiveLine in receiveLines)
        {
            var orderLine = order.Lines.SingleOrDefault(l => l.ArticleGuid == receiveLine.ArticleGuid)
                ?? throw new FargoApplicationException(
                    $"Article '{receiveLine.ArticleGuid}' is not on this order.",
                    FargoErrorType.InvalidOperation);

            var article = orderLine.Article;

            // Effect the stock change first, then register the receipt on the
            // order. Both commit in the single transaction owned by the pipeline.
            if (article.TrackingMode == ArticleTrackingMode.Entity)
            {
                ReceiveEntityTracked(article, container, receiveLine.Quantity);
            }
            else
            {
                var itemEvent = ItemEvent.NewCreationEvent(article, container, receiveLine.Quantity);

                itemEventRepository.Add(itemEvent);
            }

            order.RegisterReceipt(receiveLine.ArticleGuid, receiveLine.Quantity);
        }
    }

    private void ReceiveEntityTracked(Article article, Item? container, Scalar quantity)
    {
        var units = quantity.Value;

        // Entity-tracked stock materializes one item per unit, so the quantity
        // must be a positive whole number.
        if (units <= 0 || units != Math.Floor(units))
        {
            throw new FargoApplicationException(
                $"The received quantity for entity-tracked article '{article.Guid}' must be a positive whole number.",
                FargoErrorType.InvalidOperation);
        }

        for (var i = 0; i < (int)units; i++)
        {
            var item = Item.CreateItem(article);

            if (container is not null)
            {
                item.PlaceInsideContainer(container);
            }

            itemRepository.Add(item);
        }
    }
}
