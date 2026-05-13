using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

#region Create

public sealed record ItemCreateCommand(
    ItemCreateDto Item
) : ICommand<Guid>;

public sealed class ItemCreateCommandHandler(
    IItemRepository itemRepository,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ItemCreateCommandHandler> logger
) : ICommandHandler<ItemCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ItemCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item create flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.Item.ArticleGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.CreateItem);

        var article = await articleRepository.GetFoundByGuid(command.Item.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        var item = new Item(article)
        {
            ProductionDate = command.Item.ProductionDate
        };

        foreach (var partitionGuid in command.Item.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            item.Partitions.Add(partition);
        }

        itemRepository.Add(item);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item create flow completed for item {ItemGuid} by actor {ActorGuid}. ArticleGuid: {ArticleGuid}. PartitionCount: {PartitionCount}.",
                item.Guid,
                actor.ActorGuid,
                article.Guid,
                item.Partitions.Count);
        }

        return item.Guid;
    }
}

#endregion Create

#region Delete

public sealed record ItemDeleteCommand(
    Guid ItemGuid
) : ICommand;

public sealed class ItemDeleteCommandHandler(
    IItemRepository itemRepository,
    IUnitOfWork unitOfWork,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ItemDeleteCommandHandler> logger
) : ICommandHandler<ItemDeleteCommand>
{
    public async Task Handle(
        ItemDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item delete flow started for item {ItemGuid} by actor {ActorGuid}.",
                command.ItemGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.DeleteItem);

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        actor.ValidateHasAccess(item);

        itemRepository.Remove(item);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item delete flow completed for item {ItemGuid} by actor {ActorGuid}.",
                item.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Delete

#region Update

public sealed record ItemUpdateCommand(
    Guid ItemGuid,
    ItemUpdateDto Item
) : ICommand;

public sealed class ItemUpdateCommandHandler(
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    ItemService itemService,
    IUnitOfWork unitOfWork,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ItemUpdateCommandHandler> logger
) : ICommandHandler<ItemUpdateCommand>
{
    public async Task Handle(
        ItemUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item update flow started for item {ItemGuid} by actor {ActorGuid}.",
                command.ItemGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditItem);

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        actor.ValidateHasAccess(item);

        if (command.Item.ParentContainerGuid is { } parentContainerGuid)
        {
            var parentContainerItem = await itemRepository.GetFoundByGuid(
                parentContainerGuid,
                cancellationToken);

            actor.ValidateHasAccess(parentContainerItem);

            await itemService.MoveToContainer(
                parentContainerItem,
                item,
                cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Item update flow moved item {ItemGuid} into parent container {ParentContainerGuid}.",
                    item.Guid,
                    parentContainerItem.Guid);
            }
        }
        else
        {
            ItemService.RemoveFromContainer(item);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Item update flow removed item {ItemGuid} from its parent container.", item.Guid);
            }
        }

        #region Partition

        if (command.Item.Partitions is { } requestedPartitions)
        {
            foreach (var partitionGuid in requestedPartitions)
            {
                if (item.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                actor.ValidateHasPartitionAccess(partition.Guid);

                item.Partitions.Add(partition);
            }

            var partitionsToRemove = item.Partitions
                .Where(p => !requestedPartitions.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                actor.ValidateHasPartitionAccess(partition.Guid);

                item.Partitions.Remove(partition);
            }
        }

        #endregion Partition

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item update flow completed for item {ItemGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                item.Guid,
                actor.ActorGuid,
                item.Partitions.Count);
        }
    }
}

#endregion Update
