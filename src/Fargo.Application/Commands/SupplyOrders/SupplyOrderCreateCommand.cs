using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.SupplyOrders;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.SupplyOrders;

namespace Fargo.Application.Commands.SupplyOrders;

public sealed record SupplyOrderCreateCommand(SupplyOrderCreateDto Create) : ICommand<Guid>;

public sealed class SupplyOrderCreateCommandHandler(
    ActorResolver actorService,
    ISupplyOrderRepository supplyOrderRepository,
    IArticleRepository articleRepository,
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<SupplyOrderCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(SupplyOrderCreateCommand command, CancellationToken cancellationToken = default)
    {
        var create = command.Create;

        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreateSupplyOrder);

        var order = SupplyOrder.NewSupplyOrder(create.Reference);

        if (create.Description is { } description)
        {
            order.Description = description;
        }

        if (create.Lines is { Count: > 0 } lines)
        {
            foreach (var lineCreate in lines)
            {
                var article = await articleRepository.GetByGuidAsync(lineCreate.ArticleGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(article, lineCreate.ArticleGuid);

                actor.ThrowIfAccessDenied(article);

                order.AddLine(article, lineCreate.OrderedQuantity);
            }
        }

        if (create.ReceivingContainerGuid is { } receivingContainerGuid)
        {
            var container = await itemRepository.GetByGuidAsync(receivingContainerGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(container, receivingContainerGuid);

            actor.ThrowIfAccessDenied(container);

            // The entity validates that the item is a container.
            order.SetReceivingContainer(container);
        }

        if (create.PartitionsToAdd is { Count: > 0 } partitionGuids)
        {
            foreach (var partitionGuid in partitionGuids.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                order.AddPartition(partition);
            }
        }

        supplyOrderRepository.Add(order);

        return order.Guid;
    }
}
