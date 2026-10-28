using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemCreateCommandHandler(
    ActorResolver actorService,
    IItemRepository itemRepository,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<ItemCreateCommandHandler> logger
) : ICommandHandler<ItemCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(ItemCreateCommand command, CancellationToken cancellationToken = default)
    {
        using var scope = logger.BeginActorScope(currentActor);
        logger.CommandStarted("Item create", "article", command.Create.ArticleGuid);

        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreateItem);

        var article = await articleRepository.GetByGuidAsync(command.Create.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.Create.ArticleGuid, EntityType.Article);

        actor.ThrowIfAccessDenied(article);

        // Event-tracked articles do not materialize item rows; their stock is
        // recorded through item events instead (see the item events resource).
        var item = Item.CreateItem(article);

        var itemAudit = AuditLog.CreateAuditLog(actor, item, AuditLogType.EntityCreated);

        itemAudit.Metadata.AddArticleGuid(article.Guid);

        if (command.Create.ParentItemContainerGuid is { } parentItemContainerGuid)
        {
            var parentItemContainer = await itemRepository.GetByGuidAsync(parentItemContainerGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentItemContainer, parentItemContainerGuid, EntityType.Item);

            actor.ThrowIfAccessDenied(parentItemContainer);

            item.PlaceInsideContainer(parentItemContainer);
        }

        itemAudit.Metadata.AddParentContainer(item.ParentItemContainerGuid);

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

        itemAudit.Metadata.AddPartitions(item.PartitionGuids);

        itemRepository.Add(item);

        auditLogRepository.Add(itemAudit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CommandCompleted("Item create", "item", item.Guid);

        return item.Guid;
    }
}
