using Fargo.Application.Common;
using Fargo.Application.Operations.Items.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Operations.Items;

public sealed record ItemCreateCommand(ItemCreateDto Create) : ICommand<Guid>;

public sealed class ItemCreateCommandHandler(
    ActorResolver actorService,
    IItemRepository itemRepository,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<ItemCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(ItemCreateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreateItem);

        var article = await articleRepository.GetByGuidAsync(command.Create.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.Create.ArticleGuid);

        actor.ThrowIfAccessDenied(article);

        // Event-tracked articles do not materialize item rows; their stock is
        // recorded through item events instead (see the item events resource).
        var item = Item.CreateItem(article);

        if (command.Create.ParentItemContainerGuid is { } parentItemContainerGuid)
        {
            var parentItemContainer = await itemRepository.GetByGuidAsync(parentItemContainerGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentItemContainer, parentItemContainerGuid);

            actor.ThrowIfAccessDenied(parentItemContainer);

            item.PlaceInsideContainer(parentItemContainer);
        }

        if (command.Create.PartitionsToAdd is { Count: > 0 } partitionGuids)
        {
            foreach (var partitionGuid in partitionGuids)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                item.AddPartition(partition);
            }
        }

        itemRepository.Add(item);

        return item.Guid;
    }
}
