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
/// Command used to create a new item from an API creation payload.
/// </summary>
public sealed record ItemCreateCommand(
    ItemCreateDto Create
) : ICommand<Guid>;

/// <summary>
/// Handles item creation, including optional create-time state.
/// </summary>
public sealed class ItemCreateCommandHandler(
    IItemRepository itemRepository,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ItemCreateCommandHandler> logger
) : ICommandHandler<ItemCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ItemCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var create = command.Create;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item create flow started for article {ArticleGuid} by actor {ActorGuid}.",
                create.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(create.ArticleGuid, cancellationToken);

        var item = Item.CreateItem(article, actor, create.ProductionDate);

        itemRepository.Add(item);

        entityEventRepository.Add(EntityEvent.EntityCreated<Item>(item, actor.Guid));

        if (create.Partitions is { Count: > 0 } partitions)
        {
            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                item.AddPartition(partition, actor);

                entityPartitionEventRepository.Add(EntityPartitionEvent.InsertedIntoPartition(
                    item,
                    partition,
                    actor.Guid));
            }
        }

        if (create.IsActive == false)
        {
            item.Deactivate(actor);

            entityEventRepository.Add(EntityEvent.Deactivated<Item>(item, actor.Guid));
        }

        await unitOfWork.SaveChanges(cancellationToken);

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

#region Update

/// <summary>
/// Command used to update an existing item from an API update payload.
/// </summary>
public sealed record ItemUpdateCommand(
    Guid ItemGuid,
    ItemUpdateDto Update
) : ICommand;

/// <summary>
/// Handles item updates.
/// </summary>
public sealed class ItemUpdateCommandHandler(
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    IItemMovementRepository itemMovementRepository,
    ItemService itemService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ItemUpdateCommandHandler> logger
) : ICommandHandler<ItemUpdateCommand>
{
    public async Task Handle(
        ItemUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var update = command.Update;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item update flow started for item {ItemGuid} by actor {ActorGuid}.",
                command.ItemGuid,
                actor.Guid);
        }

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        item.ValidateCanEdit(actor);

        var previousParentContainerGuid = item.ParentContainerGuid;

        if (previousParentContainerGuid != update.ParentContainerGuid)
        {
            if (update.ParentContainerGuid is { } parentContainerGuid)
            {
                var parentContainerItem = await itemRepository.GetFoundByGuid(
                    parentContainerGuid,
                    cancellationToken);

                await itemService.MoveToContainer(
                    parentContainerItem,
                    item,
                    actor,
                    cancellationToken);
            }
            else
            {
                ItemService.RemoveFromContainer(item, actor);
            }

            itemMovementRepository.Add(ItemMovement.Moved(
                item,
                previousParentContainerGuid,
                item.ParentContainerGuid,
                actor.Guid));
        }

        var requestedPartitions = update.Partitions.Distinct().ToArray();

        var hasPartitionChanges =
            item.Partitions.Count != requestedPartitions.Length ||
            item.Partitions.Any(p => !requestedPartitions.Contains(p.Guid));

        if (hasPartitionChanges)
        {
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
        }

        if (update.IsActive is { } isActive)
        {
            if (isActive && !item.IsActive)
            {
                item.Activate(actor);
                entityEventRepository.Add(EntityEvent.Activated<Item>(item, actor.Guid));
            }
            else if (!isActive && item.IsActive)
            {
                item.Deactivate(actor);
                entityEventRepository.Add(EntityEvent.Deactivated<Item>(item, actor.Guid));
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item update mutation completed for item {ItemGuid} by actor {ActorGuid}.",
                item.Guid,
                actor.Guid);
        }
    }
}

#endregion Update

#region Delete

/// <summary>
/// Command used to delete an item.
/// </summary>
public sealed record ItemDeleteCommand(
    Guid ItemGuid
) : ICommand;

/// <summary>
/// Handles item deletion.
/// </summary>
public sealed class ItemDeleteCommandHandler(
    IItemRepository itemRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
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

        await unitOfWork.SaveChanges(cancellationToken);

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
