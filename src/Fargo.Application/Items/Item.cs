using Fargo.Application.Articles;
using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Fargo.Application.Items;

#region DTOs

public sealed record ItemDto(
    Guid Guid,
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate,
    Guid? ParentContainerGuid,
    IReadOnlyCollection<Guid> Partitions,
    Guid? EditedByGuid
);

public sealed record ItemCreateDto(
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    IReadOnlyCollection<Guid>? Partitions = null
);

public sealed record ItemUpdateDto(
    IReadOnlyCollection<Guid> Partitions,
    Guid? ParentContainerGuid = null
);

public static class ItemDtoMappings
{
    public static readonly Expression<Func<Item, ItemDto>> Projection = item => new ItemDto(
        item.Guid,
        item.ArticleGuid,
        item.ProductionDate,
        item.ParentContainerGuid,
        item.Partitions.Select(partition => partition.Guid).ToArray(),
        item.EditedByGuid);
}

#endregion DTOs

#region Exceptions

public class ItemNotFoundFargoApplicationException(Guid itemGuid)
    : FargoApplicationException($"Item with guid '{itemGuid}' was not found.")
{
    public Guid ItemGuid { get; } = itemGuid;
}

#endregion Exceptions

#region Repositories

public interface IItemQueryRepository
{
    Task<ItemDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}
public static class ItemRepositoryExtensions
{
    extension(IItemRepository repository)
    {
        public async Task<Item> GetFoundByGuid(
            Guid itemGuid,
            CancellationToken cancellationToken = default
        )
        {
            var item = await repository.GetByGuid(itemGuid, cancellationToken)
                ?? throw new ItemNotFoundFargoApplicationException(itemGuid);

            return item;
        }
    }
}

#endregion Repositories

#region Create Delete Update

#region Create

public sealed record ItemCreateCommand(
    ItemCreateDto Item
) : ICommand<Guid>;

public sealed class ItemCreateCommandHandler(
    ActorService actorService,
    IItemRepository itemRepository,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    ILogger<ItemCreateCommandHandler> logger
) : ICommandHandler<ItemCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ItemCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item create flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.Item.ArticleGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateItem);

        var article = await articleRepository.GetFoundByGuid(command.Item.ArticleGuid, cancellationToken);

        var item = new Item(article)
        {
            ProductionDate = command.Item.ProductionDate
        };

        #region Partition

        foreach (var partitionGuid in command.Item.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            item.Partitions.Add(partition);
        }

        #endregion Partition

        itemRepository.Add(item);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item create flow completed for item {ItemGuid} by actor {ActorGuid}. ArticleGuid: {ArticleGuid}. PartitionCount: {PartitionCount}.",
                item.Guid,
                actor.Guid,
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
    ActorService actorService,
    IItemRepository itemRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<ItemDeleteCommandHandler> logger
) : ICommandHandler<ItemDeleteCommand>
{
    public async Task Handle(
        ItemDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item delete flow started for item {ItemGuid} by actor {ActorGuid}.",
                command.ItemGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

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
                actor.Guid);
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
    ActorService actorService,
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    ItemService itemService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<ItemUpdateCommandHandler> logger
) : ICommandHandler<ItemUpdateCommand>
{
    public async Task Handle(
        ItemUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item update flow started for item {ItemGuid} by actor {ActorGuid}.",
                command.ItemGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

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
                actor.Guid,
                item.Partitions.Count);
        }
    }
}

#endregion Update

#endregion Create Delete Update

#region Queries

#region Single

public sealed record ItemSingleQuery(
    Guid ItemGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ItemDto?>;

public sealed class ItemSingleQueryHandler(
    ActorService actorService,
    IItemQueryRepository itemRepository,
    ICurrentUser currentUser,
    ILogger<ItemSingleQueryHandler> logger
) : IQueryHandler<ItemSingleQuery, ItemDto?>
{
    public async Task<ItemDto?> Handle(
        ItemSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Item single query started for item {ItemGuid} by actor {ActorGuid}.",
                query.ItemGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var item = await itemRepository.GetInfoByGuid(
            query.ItemGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Item single query completed for item {ItemGuid} by actor {ActorGuid}. Found: {Found}.",
                query.ItemGuid,
                actor.Guid,
                item is not null);
        }

        return item;
    }
}

#endregion Single

#region Many

public sealed record ItemsQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<ItemDto>>;

public sealed class ItemsQueryHandler(
    ActorService actorService,
    IItemQueryRepository itemRepository,
    ICurrentUser currentUser,
    ILogger<ItemsQueryHandler> logger
) : IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>>
{
    public async Task<IReadOnlyCollection<ItemDto>> Handle(
        ItemsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Items query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
                actorGuid,
                pagination.Page,
                pagination.Limit);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessesGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var items = await itemRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Items query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
                actor.Guid,
                query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0,
                items.Count);
        }

        return items;
    }
}

#endregion Many

#endregion Queries
