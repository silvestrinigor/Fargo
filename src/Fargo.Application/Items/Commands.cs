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

/// <summary>
/// Command used to create a new item.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier used as the item article.
/// </param>
/// <param name="ProductionDate">
/// Optional item production date.
/// </param>
public sealed record ItemCreateCommand(
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null
) : ICommand<Guid>;

/// <summary>
/// Handles item creation.
/// </summary>
/// <remarks>
/// Validates permissions, loads the article,
/// and stores the new item.
/// </remarks>
public sealed class ItemCreateCommandHandler(
    IItemRepository itemRepository,
    IArticleRepository articleRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
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
                command.ArticleGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.CreateItem);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        var item = new Item(article, command.ProductionDate);

        item.MarkAsEditedBy(actor.ActorGuid);

        item.MarkModificationType(ItemModifiedType.General);

        itemRepository.Add(item);

        entityEventRepository.Add(EntityEvent.EntityCreated<Item>(item, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item create mutation completed for item {ItemGuid} by actor {ActorGuid}. ArticleGuid: {ArticleGuid}.",
                item.Guid,
                actor.ActorGuid,
                article.Guid);
        }

        return item.Guid;
    }
}

#endregion Create

#region Delete

/// <summary>
/// Command used to delete an item.
/// </summary>
/// <param name="ItemGuid">
/// Item unique identifier.
/// </param>
public sealed record ItemDeleteCommand(
    Guid ItemGuid
) : ICommand;

/// <summary>
/// Handles item deletion.
/// </summary>
/// <remarks>
/// Validates permissions and removes the item.
/// </remarks>
public sealed class ItemDeleteCommandHandler(
    IItemRepository itemRepository,
    IEntityEventRepository entityEventRepository,
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

        entityEventRepository.Add(EntityEvent.EntityDeleted<Item>(item, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item delete mutation completed for item {ItemGuid} by actor {ActorGuid}.",
                item.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Delete

#region Parent Container

/// <summary>
/// Command used to set or clear the item parent container.
/// </summary>
/// <param name="ItemGuid">
/// Item unique identifier.
/// </param>
/// <param name="ParentContainerGuid">
/// Parent container item unique identifier, or <see langword="null"/>
/// to leave the item outside any container.
/// </param>
public sealed record ItemSetParentContainerCommand(
    Guid ItemGuid,
    Guid? ParentContainerGuid
) : ICommand;

/// <summary>
/// Handles item parent container changes.
/// </summary>
/// <remarks>
/// Validates permissions and either moves the item into a container
/// or clears its parent container relationship.
/// </remarks>
public sealed class ItemSetParentContainerCommandHandler(
    IItemRepository itemRepository,
    IItemMovementRepository itemMovementRepository,
    ItemService itemService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ItemSetParentContainerCommandHandler> logger
) : ICommandHandler<ItemSetParentContainerCommand>
{
    public async Task Handle(
        ItemSetParentContainerCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item parent container mutation started for item {ItemGuid} by actor {ActorGuid}.",
                command.ItemGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditItem);

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        actor.ValidateHasAccess(item);

        var previousParentContainerGuid = item.ParentContainerGuid;

        if (previousParentContainerGuid == command.ParentContainerGuid)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Item parent container mutation skipped for item {ItemGuid}; parent container is already {ParentContainerGuid}.",
                    item.Guid,
                    previousParentContainerGuid);
            }

            return;
        }

        if (command.ParentContainerGuid is { } parentContainerGuid)
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
                logger.LogInformation(
                    "Item update flow removed item {ItemGuid} from all containers; it no longer has a parent container.",
                    item.Guid);
            }
        }

        itemMovementRepository.Add(ItemMovement.Moved(
            item,
            previousParentContainerGuid,
            item.ParentContainerGuid,
            actor.ActorGuid));

        item.MarkAsEditedBy(actor.ActorGuid);

        item.MarkModificationType(ItemModifiedType.ParentContainerChanged);
    }
}

#endregion Parent Container

#region Partitions

/// <summary>
/// Command used to replace the item partition assignments.
/// </summary>
/// <param name="ItemGuid">
/// Item unique identifier.
/// </param>
/// <param name="PartitionGuids">
/// Desired partition unique identifiers.
/// </param>
public sealed record ItemSetPartitionsCommand(
    Guid ItemGuid,
    IReadOnlyCollection<Guid> PartitionGuids
) : ICommand;

/// <summary>
/// Handles item partition assignment changes.
/// </summary>
/// <remarks>
/// Validates permissions and synchronizes the item partitions
/// with the requested set.
/// </remarks>
public sealed class ItemSetPartitionsCommandHandler(
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ItemSetPartitionsCommandHandler> logger
) : ICommandHandler<ItemSetPartitionsCommand>
{
    public async Task Handle(
        ItemSetPartitionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item partition mutation started for item {ItemGuid} by actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}.",
                command.ItemGuid,
                actor.ActorGuid,
                command.PartitionGuids.Count);
        }

        actor.ValidateHasPermission(ActionType.EditItem);

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        actor.ValidateHasAccess(item);

        var requestedPartitions = command.PartitionGuids.Distinct().ToArray();

        var hasChanges =
            item.Partitions.Count != requestedPartitions.Length ||
            item.Partitions.Any(p => !requestedPartitions.Contains(p.Guid));

        if (!hasChanges)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Item partition mutation skipped for item {ItemGuid}; partitions are already requested values.",
                    item.Guid);
            }

            return;
        }

        foreach (var partitionGuid in requestedPartitions)
        {
            if (item.Partitions.Any(p => p.Guid == partitionGuid))
            {
                continue;
            }

            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            item.AddPartition(partition);

            entityPartitionEventRepository.Add(EntityPartitionEvent.InsertedIntoPartition(
                item,
                partition,
                actor.ActorGuid));
        }

        var partitionsToRemove = item.Partitions
            .Where(p => !requestedPartitions.Contains(p.Guid))
            .ToList();

        foreach (var partition in partitionsToRemove)
        {
            actor.ValidateHasPartitionAccess(partition.Guid);

            item.RemovePartition(partition);

            entityPartitionEventRepository.Add(EntityPartitionEvent.RemovedFromPartition(
                item,
                partition,
                actor.ActorGuid));
        }

        item.MarkAsEditedBy(actor.ActorGuid);

        item.MarkModificationType(ItemModifiedType.PartitionsChanged);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item partition mutation completed for item {ItemGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                item.Guid,
                actor.ActorGuid,
                item.Partitions.Count);
        }
    }
}

#endregion Partitions
