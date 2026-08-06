using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemCreateCommandHandler(
    ActorService actorService,
    IItemRepository itemRepository,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<ItemCreateCommandHandler> logger
) : ICommandHandler<ItemCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        ItemCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.CreateStarted(command.Create.ArticleGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.CreateItem);

        var article = await articleRepository.GetByGuidAsync(command.Create.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.Create.ArticleGuid, EntityType.Article);

        actor.ThrowIfAccessDenied(article);

        var item = Item.CreateItem(article);

        if (command.Create.ParentItemContainerGuid is { } parentItemContainerGuid)
        {
            var parentItemContainer = await itemRepository.GetByGuidAsync(parentItemContainerGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentItemContainer, parentItemContainerGuid, EntityType.Item);

            actor.ThrowIfAccessDenied(parentItemContainer);

            item.SetParentItemContainer(parentItemContainer);
        }

        if (command.Create.PartitionsToAdd is { Count: > 0 } partitionGuids)
        {
            foreach (var partitionGuid in partitionGuids)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                item.AddPartition(partition);
            }
        }

        itemRepository.Add(item);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(item.Guid, actor.Guid, article.Guid);

        return item.Guid;
    }
}
