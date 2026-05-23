using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Application.Workspaces;
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
    DateTimeOffset? ProductionDate = null,
    ReservedItemGuid? ItemGuid = null
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
    IReservedGuidSession reservedGuidSession,
    ILogger<ItemCreateCommandHandler> logger
) : ICommandHandler<ItemCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ItemCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item create flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var itemGuid = reservedGuidSession.ResolveItemGuid(command.ItemGuid);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        var item = Item.CreateItem(itemGuid, article, actor, command.ProductionDate);

        itemRepository.Add(item);

        entityEventRepository.Add(EntityEvent.EntityCreated<Item>(item, actor.Guid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item create mutation completed for item {ItemGuid} by actor {ActorGuid}. ArticleGuid: {ArticleGuid}.",
                item.Guid,
                actor.Guid,
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
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item delete flow started for item {ItemGuid} by actor {ActorGuid}.",
                command.ItemGuid,
                actor.Guid);
        }

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        item.ValidateCanDelete(actor);

        itemRepository.Remove(item);

        entityEventRepository.Add(EntityEvent.EntityDeleted<Item>(item, actor.Guid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item delete mutation completed for item {ItemGuid} by actor {ActorGuid}.",
                item.Guid,
                actor.Guid);
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
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item parent container mutation started for item {ItemGuid} by actor {ActorGuid}.",
                command.ItemGuid,
                actor.Guid);
        }

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        item.ValidateCanEdit(actor);

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

            await itemService.MoveToContainer(
                parentContainerItem,
                item,
                actor,
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
            ItemService.RemoveFromContainer(item, actor);
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
            actor.Guid));
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
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item partition mutation started for item {ItemGuid} by actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}.",
                command.ItemGuid,
                actor.Guid,
                command.PartitionGuids.Count);
        }

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        item.ValidateCanEdit(actor);

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

            item.AddPartition(partition, actor);

            entityPartitionEventRepository.Add(EntityPartitionEvent.InsertedIntoPartition(
                item,
                partition,
                actor.Guid));
        }

        var partitionsToRemove = item.Partitions
            .Where(p => !requestedPartitions.Contains(p.Guid))
            .ToList();

        foreach (var partition in partitionsToRemove)
        {
            item.RemovePartition(partition, actor);

            entityPartitionEventRepository.Add(EntityPartitionEvent.RemovedFromPartition(
                item,
                partition,
                actor.Guid));
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item partition mutation completed for item {ItemGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                item.Guid,
                actor.Guid,
                item.Partitions.Count);
        }
    }
}

#endregion Partitions

#region Activate

/// <summary>
/// Command used to activate an item.
/// </summary>
/// <param name="ItemGuid">
/// Item unique identifier.
/// </param>
public sealed record ItemActivateCommand(
    Guid ItemGuid
) : ICommand;

/// <summary>
/// Handles item activation.
/// </summary>
/// <remarks>
/// Validates permissions and activates the item.
/// </remarks>
public sealed class ItemActivateCommandHandler(
    IItemRepository itemRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ItemActivateCommandHandler> logger
) : ICommandHandler<ItemActivateCommand>
{
    public async Task Handle(
        ItemActivateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item activation flow started for item {ItemGuid} by actor {ActorGuid}.",
                command.ItemGuid,
                actor.Guid);
        }

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        item.ValidateCanEdit(actor);

        if (item.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Item activation flow skipped for item {ItemGuid}; item is already active.",
                    item.Guid);
            }

            return;
        }

        item.Activate(actor);

        entityEventRepository.Add(EntityEvent.Activated<Item>(item, actor.Guid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item activation mutation completed for item {ItemGuid} by actor {ActorGuid}.",
                item.Guid,
                actor.Guid);
        }
    }
}

#endregion Activate

#region Deactivate

/// <summary>
/// Command used to deactivate an item.
/// </summary>
/// <param name="ItemGuid">
/// Item unique identifier.
/// </param>
public sealed record ItemDeactivateCommand(
    Guid ItemGuid
) : ICommand;

/// <summary>
/// Handles item deactivation.
/// </summary>
/// <remarks>
/// Validates permissions and deactivates the item.
/// </remarks>
public sealed class ItemDeactivateCommandHandler(
    IItemRepository itemRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ItemDeactivateCommandHandler> logger
) : ICommandHandler<ItemDeactivateCommand>
{
    public async Task Handle(
        ItemDeactivateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item deactivation flow started for item {ItemGuid} by actor {ActorGuid}.",
                command.ItemGuid,
                actor.Guid);
        }

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        item.ValidateCanEdit(actor);

        if (!item.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Item deactivation flow skipped for item {ItemGuid}; item is already inactive.",
                    item.Guid);
            }

            return;
        }

        item.Deactivate(actor);

        entityEventRepository.Add(EntityEvent.Deactivated<Item>(item, actor.Guid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item deactivation mutation completed for item {ItemGuid} by actor {ActorGuid}.",
                item.Guid,
                actor.Guid);
        }
    }
}

#endregion Deactivate
